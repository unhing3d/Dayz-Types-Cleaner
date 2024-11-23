# Deerisle Types Editor

A Windows Forms application for batch editing DayZ mod XML type files, specifically designed for the Deerisle mod configuration.

## Features

- Bulk edit multiple XML files simultaneously
- Modify key item properties:
  - Lifetime values
  - Restock intervals
  - Nominal values
  - Minimum values
- Category-based filtering with preset categories:
  - Weapons
  - Clothes
  - Food
  - Tools
  - Containers
  - Industrial Food
  - Explosives
  - Loot Dispatch
  - KMUCKeycard

## Usage

1. Launch the application
2. Select desired categories from the checklist
3. Set your preferred values for:
   - Lifetime Value (default: 7200)
   - Restock Value (default: 1800)
   - Nominal Value
   - Min Value
4. Click "Select Files and Update" to choose your XML files
5. Modified files will be saved in an "updates" folder in the application directory

## Technical Details

- Built with .NET Windows Forms
- Processes XML files using LINQ to XML
- Supports multi-file selection
- Includes progress tracking and error logging
- Maintains original file structure while updating specified values

## System Requirements

- Windows Operating System
- .NET Framework Runtime

## Installation

1. Download the latest release
2. Extract all files to your preferred location
3. Run DeerisleEditor.exe

## Output

Modified files are automatically saved to an "updates" subdirectory, preserving your original files.

## Notes

- Always backup your original XML files before processing
- The application validates XML structure during processing
- Progress and errors are logged in the application window
