using RobotProgramming.Core;
using System.Collections.Generic;
using System.Linq;

namespace RobotProgramming.Execution
{
    public class ProgramSequence
    {
        private Dictionary<int, ICommand> commandsById;
        private ICommand startCommand;

        public ICommand StartCommand => startCommand;
        public int CommandCount => commandsById.Count;

        public ProgramSequence()
        {
            commandsById = new Dictionary<int, ICommand>();
            startCommand = null;
        }

        public void CheckSnappedCommand(ICommand command)
        {
            if (command.Id == startCommand.Id)
            {
                var temp = commandsById.Values.ToList();
                foreach (var com in commandsById.Values)
                {
                    if (temp.Contains(com.Next))
                        temp.Remove(com);
                }
                if (temp.Count > 0)
                    startCommand = temp[0];
            }
        }

        public void AddCommand(ICommand command)
        {
            if (command == null) return;

            commandsById[command.Id] = command;

            if (startCommand == null)
            {
                startCommand = command;
            }
        }

        public void LinkCommands(int fromId, int toId)
        {
            if (commandsById.TryGetValue(fromId, out ICommand from) &&
                commandsById.TryGetValue(toId, out ICommand to))
            {
                from.Next = to;
            }
        }

        public void SetStartCommand(int commandId)
        {
            if (commandsById.TryGetValue(commandId, out ICommand command))
            {
                startCommand = command;
            }
        }

        public ICommand GetCommandById(int id)
        {
            return commandsById.TryGetValue(id, out ICommand command) ? command : null;
        }

        public void Clear()
        {
            commandsById.Clear();
            startCommand = null;
        }
    }
}
