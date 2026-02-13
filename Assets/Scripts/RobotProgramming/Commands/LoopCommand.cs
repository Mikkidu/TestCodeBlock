using PU.Promises;
using RobotProgramming.Core;
using RobotProgramming.Data;
using RobotProgramming.UI;
using UnityEngine;

namespace RobotProgramming.Commands
{
    public class LoopCommand : CommandBase
    {
        public override CommandType Type => CommandType.Loop;

        private LoopBlockUI loopBlockUI;
        private int currentIteration = 0;
        private bool shouldStop = false;

        public LoopCommand(int id) : base(id, new float[] { })
        {
        }

        public void SetLoopBlockUI(LoopBlockUI ui)
        {
            loopBlockUI = ui;
        }

        /// <summary>
        /// Основной метод выполнения - вызывается когда управление входит в цикл извне
        /// </summary>
        public override IPromise Execute(IRobotController robot, ExecutionContext context)
        {
            Debug.Log("[LOOP] Entering from EXTERNAL input");
            currentIteration = 0;
            shouldStop = false;
            return ExecuteIteration(robot, context);
        }

        /// <summary>
        /// Вызывается когда управление вернулось из последнего блока внутри цикла
        /// (через внутренний INPUT коннектор)
        /// </summary>
        public IPromise ExecuteFromInternalInput(IRobotController robot, ExecutionContext context)
        {
            Debug.Log($"[LOOP] Returned from iteration {currentIteration}");
            return ExecuteIteration(robot, context);
        }

        /// <summary>
        /// Внутренняя логика: проверка условия, выполнение блоков или переход дальше
        /// </summary>
        private IPromise ExecuteIteration(IRobotController robot, ExecutionContext context)
        {
            Debug.Log($"[LOOP] ExecuteIteration called (iteration {currentIteration})");

            // Проверка условия остановки
            if (shouldStop)
            {
                Debug.Log($"[LOOP] Stopped after {currentIteration} iterations");
                return ContinueAfterLoop(robot, context);
            }

            // Проверка условия цикла (пока бесконечный - всегда true)
            if (!CheckCondition())
            {
                Debug.Log($"[LOOP] Condition false, exiting after {currentIteration} iterations");
                return ContinueAfterLoop(robot, context);
            }

            // Получить первый блок внутри цикла
            if (loopBlockUI == null)
            {
                Debug.LogError("[LOOP] loopBlockUI is NULL!");
                return ContinueAfterLoop(robot, context);
            }

            BlockUIBase firstInner = loopBlockUI.GetFirstInnerBlock();
            Debug.Log($"[LOOP] GetFirstInnerBlock returned: {(firstInner != null ? firstInner.gameObject.name : "NULL")}");

            if (firstInner == null)
            {
                Debug.Log("[LOOP] Empty loop, continuing to next block");
                return ContinueAfterLoop(robot, context);
            }

            // Начало новой итерации
            currentIteration++;
            Debug.Log($"[LOOP] Starting iteration {currentIteration}");
            context.SetVariable("loop_iteration", currentIteration - 1);

            // Передать управление первому блоку внутри
            // Блоки будут выполняться по цепочке
            // Когда последний блок вернёт управление через внутренний input коннектор,
            // должен быть вызван специальный обработчик
            return ExecuteInnerChain(firstInner, robot, context);
        }

        /// <summary>
        /// Выполнить цепочку блоков внутри цикла
        /// </summary>
        private IPromise ExecuteInnerChain(BlockUIBase startBlock, IRobotController robot, ExecutionContext context)
        {
            if (startBlock == null)
            {
                Debug.Log("[LOOP] No inner blocks to execute");
                return ExecuteIteration(robot, context);
            }

            ICommand command = startBlock.Command;

            if (command == null)
            {
                Debug.LogError("[LOOP] Block has no command");
                return Deferred.Rejected(new System.Exception("Block has no command"));
            }

            Deferred chainDeferred = Deferred.GetFromPool();

            command.Execute(robot, context)
                .Done(() =>
                {
                    if (shouldStop)
                    {
                        Debug.Log("[LOOP] Stop requested during iteration");
                        chainDeferred.Resolve();
                    }
                    else
                    {
                        BlockUIBase nextBlock = startBlock.GetNextBlock();

                        if (nextBlock != null)
                        {
                            // Есть следующий блок в цепочке внутри цикла
                            ExecuteInnerChain(nextBlock, robot, context)
                                .Done(() => chainDeferred.Resolve())
                                .Fail(ex => chainDeferred.Reject(ex));
                        }
                        else
                        {
                            // Это был последний блок внутри цикла
                            // Вернуться к началу цикла через ExecuteIteration
                            Debug.Log("[LOOP] Last block in chain, returning to loop");
                            ExecuteIteration(robot, context)
                                .Done(() => chainDeferred.Resolve())
                                .Fail(ex => chainDeferred.Reject(ex));
                        }
                    }
                })
                .Fail(ex => chainDeferred.Reject(ex));

            return chainDeferred;
        }

        /// <summary>
        /// Продолжить выполнение после выхода из цикла
        /// </summary>
        private IPromise ContinueAfterLoop(IRobotController robot, ExecutionContext context)
        {
            BlockUIBase nextBlock = loopBlockUI.GetNextBlock();

            if (nextBlock != null && nextBlock.Command != null)
            {
                Debug.Log("[LOOP] Continuing to next block after loop");
                return nextBlock.Command.Execute(robot, context);
            }

            Debug.Log("[LOOP] No more blocks after loop");
            return Deferred.Resolved();
        }

        /// <summary>
        /// Проверить условие цикла
        /// Пока это всегда true (бесконечный цикл)
        /// В будущем здесь будет: iteration < maxCount, условие переменной, и т.д.
        /// </summary>
        private bool CheckCondition()
        {
            // Бесконечный цикл - всегда true
            // Можно выйти только через Stop button или очистку цикла
            return true;
        }

        /// <summary>
        /// Остановить цикл (вызывается кнопкой Stop)
        /// </summary>
        public void RequestStop()
        {
            Debug.Log("[LOOP] Stop requested");
            shouldStop = true;
        }

        public override string GetDisplayName() => "Цикл (∞)";

        public override Color GetBlockColor() => new Color(0.8f, 0.4f, 0.8f); // Purple
    }
}
