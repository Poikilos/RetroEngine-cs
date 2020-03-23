# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).


## [Unreleased] - 2008-11-12
### Fixed
- Fix `thepixone` font to include black line to enclose the top of the
  double music note character.


## [Unreleased] - 2008-11
### Changed
- Eliminate Base.cs.
  - Move features to RMath, RString, etc.
  - Move remaining methods to RPlatform.cs and make
    RPlatform.*.cs files partial class files for different platforms.


## [Unreleased] - 2008-11-05
### Fixed
- `var.GetProperty` now gets the property (case-insensitive) instead of
  getting the case-sensitive onmousedown property!


## [Unreleased] - 2008-10-22
### Changed
- IRound return from `(int)(val+.5f);` to `val<0?(int)(val-.5f):(int)(val+.5f);`

