# Changelog

All notable changes to this project will be documented in this file.

The format loosely follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to Semantic Versioning.

## [1.2.1] – 2025-11-29
### Fixed
- Sample scene camera configuration

## [1.2.0] – 2025-11-29
### Added
- Trackball rotation handle (free rotation)

### Changed
- Unified interaction behaviour between scale and translation handles
- Improved prioritization of plane translation over tip and shaft selection
- Improved hover detection for scale handles

### Fixed
- Center scale functionality

### Internal
- General cleanup and robustness improvements

## [1.1.0] – 2025-08-03
### Added
- `OnHoverEnter` and `OnHoverExit` events in `TransformHandleManager`
- `SetHandlesEnabled(bool)` to enable or disable handle interaction

## [1.0.0] – 2025-07-01
### Added
- Initial release with basic translate, rotate, and scale handles
