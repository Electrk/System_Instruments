// More gross disabling/enabling stuff -- sorry!

function clientCmdInstruments_receiveFileListStart() {
  InstrumentsFileManager.clearFiles();
  InstrumentsClient.setFileListMode("root");
}

function clientCmdInstruments_receiveFileList(%list) {
  %count = getRecordCount(%list);

  for (%r = 0; %r < %count; %r++) {
    %row = getRecord(%list, %r);
    %name = getField(%row, 0);
    %type = getField(%row, 1);
    %author = getField(%row, 2);

    InstrumentsClient.schedule(1, addServerFile, %name, %type, %author);
  }
}

function clientCmdInstruments_receiveFileListDone(%type) {
  InstrumentsClient.setFileListMode("root");
}

function clientCmdInstruments_updateFileName(%old, %new, %type) {
  if (%type $= "phrase") {
    %id = InstrumentsFileManager.getPhraseID(%old);
    InstrumentsFileManager.setPhraseName(%new);
  }
  else if (%type $= "song") {
    %id = InstrumentsFileManager.getSongID(%old);
    InstrumentsFileManager.setSongName(%new); 
  }
  else if (%type $= "bindset") {
    %id = InstrumentsFileManager.getBindsetID(%old);
    InstrumentsFileManager.setBindsetName(%new); 
  }

  InstrumentsClient.refreshFileLists();
}

function clientCmdInstruments_onFileAdded(%name, %type, %author) {
  InstrumentsClient.addServerFile(%name, %type, %author);
  InstrumentsClient.refreshFileLists();
}

function clientCmdInstruments_onFileDeleted(%name, %type) {
  if (%type $= "phrase") {
    InstrumentsFileManager.deletePhrase(%name);
  }
  else if (%type $= "song") {
    InstrumentsFileManager.deleteSong(%name);
  }
  else if (%type $= "bindset") {
    InstrumentsFileManager.deleteBindset(%name);
  }

  InstrumentsClient.refreshFileLists();
}

function clientCmdInstruments_onFileRenamed(%name, %newName, %type) {
  if (%type $= "phrase") {
    %id = InstrumentsFileManager.getPhraseID(%name);
    InstrumentsFileManager.setPhraseName(%id, %newName);
  }
  else if (%type $= "song") {
    %id = InstrumentsFileManager.getSongID(%name);
    InstrumentsFileManager.setSongName(%id, %newName);
  }
  else if (%type $= "bindset") {
    %id = InstrumentsFileManager.getBindsetID(%name);
    InstrumentsFileManager.setBindsetName(%id, %newName);
  }

  InstrumentsClient.refreshFileLists();
}


function InstrumentsClient::populateFileList(%this, %list, %type, %localOrServer) {
  if (%type !$= "phrases" && %type !$= "songs" && %type !$= "bindsets" && %type !$= "root") {
    return;
  }

  if (!isObject(%list)) {
    return;
  }

  %list.clear();
  %list.setSelectedRow(-1);

  if (%type $= "root") {
    %id = -1;
    %list.addRow(%id++, "Phrases");

    if ($Instruments::Client::CanUseSongs) {
      %list.addRow(%id++, "Songs");
    }

    if ($Instruments::Client::CanUseBindsets) {
      %list.addRow(%id++, "Bindsets");
    }

    return;
  }

  if (%localOrServer $= "local") {
    %id = 0;
    %path = "config/client/instruments/" @ %type @ "/*.txt";

    for (%file = findFirstFile(%path); %file !$= ""; %file = findNextFile(%path)) {
      %filename = fileBase(%file);

      if (_strEmpty(%filename)) {
        continue;
      }

      %list.addRow(%id, %filename);
      %id++;
    }
  }
  else if (%localOrServer $= "server") {
    if (%type $= "phrases") {
      %count = InstrumentsFileManager._phraseCount;
    }
    else if (%type $= "songs") {
      %count = InstrumentsFileManager._songCount;
    }
    else if (%type $= "bindsets") {
      %count = InstrumentsFileManager._bindsetCount;
    }

    %id = 0;

    for (%i = 0; %i < %count; %i++) {
      if (%type $= "phrases") {
        %file = InstrumentsFileManager.getPhraseName(%i);
      }
      else if (%type $= "songs") {
        %file = InstrumentsFileManager.getSongName(%i);
      }
      else if (%type $= "bindsets") {
        %file = InstrumentsFileManager.getBindsetName(%i);
      }

      if (_strEmpty(%file)) {
        continue;
      }

      %list.addRow(%id, %file);
      %id++;
    }
  }
  else {
    error("[fileList.cs] InstrumentsClient::populateFileList() - %localOrServer is invalid!");
  }

  if (%type !$= "root") {
    // Sort alphabetically
    %list.sort(0, 1);

    // This is used to go back to the main view
    %list.insertBefore(0, "<< Back");
  }
}

function InstrumentsClient::addServerFile(%this, %name, %type, %author) {
  if (%type $= "phrase") {
    %id = InstrumentsFileManager._phraseCount;
    InstrumentsFileManager.addPhrase(%id, %name, %author);
  }
  else if (%type $= "song") {
    %id = InstrumentsFileManager._songCount;
    InstrumentsFileManager.addSong(%id, %name, %author);
  }
  else if (%type $= "bindset") {
    %id = InstrumentsFileManager._bindsetCount;
    InstrumentsFileManager.addBindset(%id, %name, %author);
  }
}

function InstrumentsClient::clickFileList(%this, %localOrServer) {
  %list = "InstrumentsDlg_" @ capitalizeFirstLetter(%localOrServer) @ "FileList";

  if (!isObject(%list)) {
    return;
  }

  %id = %list.getSelectedId();
  %item = %list.getRowTextById(%id);

  if (%localOrServer $= "local") {
    InstrumentsDlg_ServerFileList.setSelectedRow(-1);

    InstrumentsDlg_LoadServerFile.disable();
    InstrumentsDlg_RenameServerFile.disable();
    InstrumentsDlg_DeleteServerFile.disable();
    InstrumentsDlg_DownloadFile.disable();

    InstrumentsDlg_LoadLocalFile.enable();
    InstrumentsDlg_RenameLocalFile.enable();
    InstrumentsDlg_DeleteLocalFile.enable();

    InstrumentsDlg_UploadFile.doDisableCheck(!$Instruments::Client::CanUseSaving);
  }
  else {
    InstrumentsDlg_LocalFileList.setSelectedRow(-1);

    InstrumentsDlg_LoadLocalFile.disable();
    InstrumentsDlg_RenameLocalFile.disable();
    InstrumentsDlg_DeleteLocalFile.disable();
    InstrumentsDlg_UploadFile.disable();

    %mode = $Instruments::GUI::FileListMode;

    if (%mode !$= "root") {
      if (%mode $= "phrases") {
        %author = InstrumentsFileManager.getPhraseAuthor(%item);
      }
      else if (%mode $= "songs") {
        %author = InstrumentsFileManager.getSongAuthor(%item);
      }
      else if (%mode $= "bindsets") {
        %author = InstrumentsFileManager.getBindsetAuthor(%item);
      }

      %bl_id = getField(%author, 1);

      if ((%bl_id != getNumKeyID() && $IamAdmin <= 0) || !$Instruments::Client::CanUseDeleting) {
        InstrumentsDlg_RenameServerFile.disable();
        InstrumentsDlg_DeleteServerFile.disable();
      }
      else {
        InstrumentsDlg_RenameServerFile.enable();
        InstrumentsDlg_DeleteServerFile.enable();
      }

      InstrumentsDlg_LoadServerFile.doDisableCheck(!$Instruments::Client::CanUseLoading);
      InstrumentsDlg_DownloadFile.doDisableCheck(!$Instruments::Client::CanUseLoading);
    }
  }

  if (%list.lastClickTime $= "" || getSimTime() - %list.lastClickTime > 500 || %id !$= %list.lastSelected) {
    %list.lastClickTime = getSimTime();
    %list.lastSelected = %id;
  }
  else {
    %list.lastClickTime = "";

    if ($Instruments::GUI::FileListMode $= "root") {
      InstrumentsClient.setFileListMode(strLwr(%item));
    }
    else {
      if (%item $= "<< Back") {
        InstrumentsClient.setFileListMode("root");
      }
    }
  }
}

function InstrumentsClient::setFileListMode(%this, %mode) {
  $Instruments::GUI::FileListMode = %mode;

  InstrumentsClient.populateFileList(InstrumentsDlg_ServerFileList, %mode, "server");
  InstrumentsClient.populateFileList(InstrumentsDlg_LocalFileList, %mode, "local");

  if (%mode $= "root") {
    %label = "File Browser";

    InstrumentsDlg_LoadServerFile.disable();
    InstrumentsDlg_DeleteServerFile.disable();
    InstrumentsDlg_RenameServerFile.disable();

    InstrumentsDlg_LoadLocalFile.disable();
    InstrumentsDlg_DeleteLocalFile.disable();
    InstrumentsDlg_RenameLocalFile.disable();

    InstrumentsDlg_DownloadFile.disable();
    InstrumentsDlg_UploadFile.disable();

    InstrumentsDlg_RefreshServerFiles.visible = false;
    InstrumentsDlg_RefreshLocalFiles.visible = false;
  }
  else {
    %label = capitalizeFirstLetter(%mode) @ ": ";

    InstrumentsDlg_SaveServerFile.doDisableCheck(!$Instruments::Client::CanUseSaving);
    InstrumentsDlg_LoadServerFile.disable();
    InstrumentsDlg_DeleteServerFile.disable();
    InstrumentsDlg_RenameServerFile.disable();

    InstrumentsDlg_LoadLocalFile.disable();
    InstrumentsDlg_DeleteLocalFile.disable();
    InstrumentsDlg_RenameLocalFile.disable();

    InstrumentsDlg_DownloadFile.disable();
    InstrumentsDlg_UploadFile.disable();

    InstrumentsDlg_RefreshServerFiles.visible = true;
    InstrumentsDlg_RefreshLocalFiles.visible = true;
  }

  InstrumentsDlg_ServerFileBrowser.setText("<font:Impact:18>Server " @ %label);
  InstrumentsDlg_LocalFileBrowser.setText("<font:Impact:18>Local " @ %label);

  InstrumentsClient.updateSaveButtons();
}

function InstrumentsClient::refreshFileLists(%this) {
  InstrumentsClient.setFileListMode($Instruments::GUI::FileListMode);
}

function InstrumentsClient::getSelectedFile(%this) {
  %localSelected = InstrumentsDlg_LocalFileList.getSelectedId();
  %serverSelected = InstrumentsDlg_ServerFileList.getSelectedId();

  if (%localSelected < 0 && %serverSelected < 0) {
    return "";
  }

  if (%localSelected >= 0) {
    return InstrumentsDlg_LocalFileList.getRowTextById(%localSelected);
  }

  if (%serverSelected >= 0) {
    return InstrumentsDlg_ServerFileList.getRowTextById(%serverSelected);
  }
}

function InstrumentsClient::updateSaveButtons(%this) {
  %mode = $Instruments::GUI::FileListMode;

  if (%mode $= "songs") {
    InstrumentsClient.doSaveButtonsCheck(InstrumentsDlg_SongOrderList.rowCount() > 0);
  }
  else if (%mode $= "phrases") {
    InstrumentsClient.doSaveButtonsCheck(true);
    // Doesn't work if you copy and paste something into the phrase textbox
    // InstrumentsClient.doSaveButtonsCheck(strLen(InstrumentsClient.getPhrase()) > 0);
  }
  else if (%mode $= "bindsets") {
    InstrumentsClient.doSaveButtonsCheck(InstrumentsClient.binds.bindCount >= 3);
  }
  else {
    InstrumentsClient.doSaveButtonsCheck(false);
  }

  if (!$Instruments::Client::CanUseSaving) {
    InstrumentsDlg_SaveServerFile.disable();
  }
}

function InstrumentsClient::doSaveButtonsCheck(%this, %bool) {
  if (%bool) {
    InstrumentsDlg_SaveServerFile.enable();
    InstrumentsDlg_SaveLocalFile.enable();
  }
  else {
    InstrumentsDlg_SaveServerFile.disable();
    InstrumentsDlg_SaveLocalFile.disable();
  }
}
