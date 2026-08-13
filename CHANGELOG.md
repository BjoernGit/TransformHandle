# Changelog

All notable changes to this project will be documented in this file.

The format loosely follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/)
and this project adheres to Semantic Versioning.

## [Unreleased]
### Fixed
- Uniform scale centre could be hovered and dragged even when disabled in the handle profile

### Removed
- Grey axis cross at the centre of the translation handle: it was always drawn along the world axes and therefore ignored both the handle space and the target's rotation

## [1.3.0] – 2026-07-01
### Added
- Support for both the legacy Input Manager and the new Input System package via the new `HandleInput` abstraction — setting *Active Input Handling* to *Both* is no longer required

### Changed
- `IDragHandler.StartDrag` now receives the handle scale computed by the manager; drag handlers no longer estimate their own scale (plane offsets and rotation radii now match the rendered handle size)
- Single-axis translation and scale drags use a world-space ray-to-axis projection: exact 1:1 cursor tracking, immune to viewport rects / split-screen setups and view angle

### Fixed
- Objects could be dragged in the wrong direction along an arrow at certain view angles or with non-fullscreen camera viewports
- Handles rendered but did not react to input when no camera was explicitly assigned (interaction ignored the `Camera.main` fallback)
- Scale drag ignored the handle space and always projected onto local axes
- Mixed-space profiles: dragging now uses the space of the element that was actually hovered instead of always preferring Local
- Handle geometry was rendered twice per frame due to a broken batcher ownership check
- `OnTransformModified` fired every frame during a drag even when the transform did not change

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
