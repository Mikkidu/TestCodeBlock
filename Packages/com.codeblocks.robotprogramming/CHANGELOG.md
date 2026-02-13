# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-01-21

### Added
- Initial release
- BlockUIBase architecture with unified connector system
- BlockUI for simple command blocks
- LoopBlockUI for loop blocks with 4 connectors
- BlockDragHandler for drag-and-drop functionality
- SnapManager with simplified API
- Visual snap line feedback
- Command execution system with Promises
- Level Editor with JSON export/import
- 5 tutorial levels

### Architecture
- Dictionary<string, BlockConnector> for flexible connector access
- Inheritance-based block types (BlockUI, LoopBlockUI : BlockUIBase)
- Separated drag logic into BlockDragHandler component
