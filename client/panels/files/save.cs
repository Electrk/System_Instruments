function InstrumentsClient::clickSaveFile(%this, %localOrServer) {
  %mode = $Instruments::GUI::FileListMode;

  if (%mode $= "Phrases") {
    %mode = "phrase";
  }
  else if (%mode $= "Songs") {
    %mode = "song";
  }
  else if (%mode $= "Bindsets") {
    %mode = "bindset";
  }
  else {
    return;
  }

  InstrumentsClient.openSaveDialog(%mode, %localOrServer);
}

function InstrumentsClient::openSaveDialog(%this, %type, %localOrServer) {
  if (%type $= "phrase" && _strEmpty(InstrumentsClient.getPhrase())) {
    Instruments.messageBoxOK("Error", "No phrase to save!");
    return;
  }

  if (%type $= "song" && InstrumentsDlg_SongOrderList.rowCount() <= 0) {
    Instruments.messageBoxOK("Error", "No song to save!");
    return;
  }

  // <3
  if (%type $= "bindset" && InstrumentsClient.binds.bindCount < 3) {
    Instruments.messageBoxOK("Error", "Not enough binds to save!");
    return;
  }

  %authorName = $Instruments::GUI::LoadedAuthorName[%type];

  if (%authorName $= "") {
    %authorName = $Pref::Player::NetName;
  }

  %authorBL_ID = $Instruments::GUI::LoadedAuthorBL_ID[%type];

  if (%authorBL_ID $= "") {
    %authorBL_ID = getNumKeyID();
  }

  $Instruments::GUI::FileType = %type;
  $Instruments::GUI::FileLocalOrServer = %localOrServer;

  InstrumentsSaveDlg_Filename.setValue("");
  InstrumentsSaveDlg_AuthorName.setValue(%authorName);
  InstrumentsSaveDlg_AuthorBL_ID.setValue(%authorBL_ID);

  Canvas.pushDialog(InstrumentsSaveDlg);
}

function InstrumentsClient::clickSaveButton(%this) {
  %filename = InstrumentsSaveDlg_Filename.getValue();
  %authorName = InstrumentsSaveDlg_AuthorName.getValue();
  %authorBL_ID = InstrumentsSaveDlg_AuthorBL_ID.getValue();
  %author = %authorName TAB %authorBL_ID;

  Canvas.popDialog(InstrumentsSaveDlg);

  InstrumentsClient.saveFile($Instruments::GUI::FileType, $Instruments::GUI::FileLocalOrServer, %filename, %author);
}

function InstrumentsClient::saveFile(%this, %type, %localOrServer, %filename, %author) {
  if (!Instruments.validateFilename(%filename, "", 1)) {
    return;
  }

  if (%type $= "phrase") {
    if ($Instruments::GUI::isUploading) {
      %phraseOrSong = $Instruments::GUI::Upload::Phrase;
      %localOrServer = "server";
    }
    else if ($Instruments::GUI::isDownloading) {
      %phraseOrSong = $Instruments::GUI::Download::Phrase;
      %localOrServer = "local";
    }
    else {
      %phraseOrSong = InstrumentsClient.getPhrase();
    }

    %phraseOrSong = _cleanPhrase(%phraseOrSong);
  }
  else if (%type $= "song") {
    if ($Instruments::GUI::isUploading) {
      %phraseOrSong = $Instruments::GUI::Upload::Song;
      %localOrServer = "server";
    }
    else if ($Instruments::GUI::isDownloading) {
      %phraseOrSong = $Instruments::GUI::Download::Song;
      %localOrServer = "local";
    }
    else {
      %phraseOrSong = InstrumentsClient.songToText();
    }

    %phraseOrSong = _cleanPhrase(%phraseOrSong);
  }

  if (%author $= "") {
    %author = $Pref::Player::NetName TAB getNumKeyID();
  }

  if (%localOrServer $= "local") {
    Instruments.saveFile(%type, %filename, %phraseOrSong, "", 0, %author, $Instruments::GUI::isUploading);
  }
  else if (%localOrServer $= "server") {
    commandToServer('Instruments_SaveFile', %type, %filename, %phraseOrSong, 0, %author, $Instruments::GUI::isUploading);
  }

  Canvas.popDialog(InstrumentsEditTextDlg);
}

function InstrumentsClient::onFileSaveStart(%this, %type, %filename) {
  // callback
}

function InstrumentsClient::onPhraseSaved(%this, %filename, %authorName, %authorBL_ID) {
  Instruments.messageBoxOK("File Saved", "File saved successfully.");
  $Instruments::GUI::isUploading = false;
}

function InstrumentsClient::onSongSaved(%this, %filename, %authorName, %authorBL_ID) {
  Instruments.messageBoxOK("File Saved", "File saved successfully.");
  $Instruments::GUI::isUploading = false;
}

function InstrumentsClient::onBindsetSaved(%this, %filename, %authorName, %authorBL_ID) {
  Instruments.messageBoxOK("File Saved", "File saved successfully.");
  $Instruments::GUI::isUploading = false;
}

function InstrumentsClient::onFileSaveError(%this, %type, %filename, %unusedArg, %failure) {
  Instruments.messageBoxOK("Write Error", %failure);
  $Instruments::GUI::isUploading = false;
}

function clientCmdInstruments_onFileSaveStart(%type, %filename) {
  InstrumentsClient.onFileSaveStart(%type, %filename);
}

function clientCmdInstruments_onPhraseSaved(%filename, %authorName, %authorBL_ID) {
  InstrumentsClient.onPhraseSaved(%filename, %authorName, %authorBL_ID);
}

function clientCmdInstruments_onSongSaved(%filename, %authorName, %authorBL_ID) {
  InstrumentsClient.onSongSaved(%filename, %authorName, %authorBL_ID);
}

function clientCmdInstruments_onBindsetSaved(%filename, %authorName, %authorBL_ID) {
  InstrumentsClient.onBindsetSaved(%filename, %authorName, %authorBL_ID);
}

function clientCmdInstruments_onFileSaveError(%type, %filename, %failure) {
  InstrumentsClient.onFileSaveError(%type, %filename, "", %failure);
}
