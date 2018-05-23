exec("./Support_UpdaterDownload.cs");

exec("./init.cs");
exec("./profiles.cs");

if (!isObject(InstrumentsDlg)) {
  exec("./InstrumentsDlg.gui");
}

if (!isObject(InstrumentsEditTextDlg)) {
  exec("./InstrumentsEditTextDlg.gui");
}

if (!isObject(InstrumentsSaveDlg)) {
  exec("./InstrumentsSaveDlg.gui");
}

if (!isObject(InstrumentsKeybindDlg)) {
  exec("./InstrumentsKeybindDlg.gui");
}

exec("./methods.cs");
exec("./modules.cs");
exec("./miscellaneous.cs");

// For the "Piano" section of the GUI
exec("./panels/piano/notes.cs");
exec("./panels/piano/phrase.cs");
exec("./panels/piano/instrumentList.cs");
exec("./panels/piano/miscellaneous.cs");

// For the "Songs" section of the GUI
exec("./panels/songs/songPhrases.cs");
exec("./panels/songs/songOrderList.cs");

// For the "Keybinds" section of the GUI
exec("./panels/keybinds/instrumentsMap.cs");
exec("./panels/keybinds/keybindDialog.cs");
exec("./panels/keybinds/keyboard.cs");
exec("./panels/keybinds/loadKeyboardLayout.cs");
// exec("./panels/keybinds/keyboardLayouts.cs");

// For the "Files" section of the GUI
exec("./panels/files/fileManager.cs");
exec("./panels/files/fileList.cs");
exec("./panels/files/save.cs");
exec("./panels/files/load.cs");
exec("./panels/files/delete.cs");
exec("./panels/files/uploadDownload.cs");

// For the "Help" section of the GUI
exec("./panels/help/helpCategories.cs");

// Packages
exec("./packages.cs");

// -------------------------------

InstrumentsClient.loadKeyboardLayout("Add-Ons/System_Instruments/client/panels/keybinds/layouts/QWERTY.txt");
