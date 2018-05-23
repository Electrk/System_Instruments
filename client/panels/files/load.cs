function InstrumentsClient::clickLoadFile(%this, %localOrServer) {
  %type = $Instruments::GUI::FileListMode;

  if (%type $= "phrases") {
    %type = "phrase";
  }
  else if (%type $= "songs") {
    %type = "song";
  }
  else if (%type $= "bindsets") {
    %type = "bindset";
  }
  else {
    return;
  }

  %filename = _cleanFilename(InstrumentsClient.getSelectedFile());
  InstrumentsClient.loadFile(%type, %filename, %localOrServer);
}

function InstrumentsClient::loadFile(%this, %type, %filename, %localOrServer) {
  %filename = _cleanFilename(%filename);
  %phraseOrSong = "";

  $Instruments::GUI::isLoading = true;

  if (%localOrServer $= "local") {
    Instruments.loadFile(%type, %filename);
  }
  else if (%localOrServer $= "server") {
    commandToServer('Instruments_LoadFile', %type, %filename, $Instruments::GUI::isDownloading);
  }
}

function InstrumentsClient::setLoadedAuthor(%this, %type, %author, %bl_id) {
  $Instruments::GUI::LoadedAuthorName[%type] = %author;
  $Instruments::GUI::LoadedAuthorBL_ID[%type] = %bl_id;
}

function InstrumentsClient::onFileLoadStart(%this, %type, %filename, %useServerCmd) {
  if ($Instruments::GUI::isUploading || $Instruments::GUI::isDownloading || !$Instruments::GUI::isLoading) {
    return;
  }

  if (%type $= "song") {
    InstrumentsClient.clearAllSongPhrases(%useServerCmd);
  }
  else if (%type $= "bindset") {
    InstrumentsClient.clearAllKeys(%useServerCmd);
  }
}

// These methods aren't DRY at all, but I can't be fucked to rewrite them right now

function InstrumentsClient::onPhraseLoaded(%this, %phrase, %filename, %author, %bl_id, %unusedArg, %failure) {
  if (!$Instruments::GUI::isLoading) {
    return;
  }

  if (%failure $= "") {
    if ($Instruments::GUI::isUploading) {
      $Instruments::GUI::Upload::Phrase = %phrase;
      
      InstrumentsClient.saveFile("phrase", "server", %filename, %author TAB %bl_id);
      $Instruments::GUI::isUploading = false;
      deleteVariables("$Instruments::GUI::Upload::*");
    }
    else if ($Instruments::GUI::isDownloading) {
      $Instruments::GUI::Download::Phrase = %phrase;

      InstrumentsClient.saveFile("phrase", "local", %filename, %author TAB %bl_id);
      $Instruments::GUI::isDownloading = false;
      deleteVariables("$Instruments::GUI::Download::*");
    }
    else {
      %phrase = _cleanPhrase(%phrase);
      InstrumentsClient.setPhrase(%phrase);

      %body = "Successfully loaded " @ %filename @ " by " @ %author;

      if (%bl_id >= 0 && %bl_id != 888888 && %bl_id != 999999) {
         %body = %body @ " (BL_ID: " @ %bl_id @ ")";
      }

      InstrumentsClient.setLoadedAuthor("phrase", %author, %bl_id);
      InstrumentsClient.updateSaveButtons();

      Instruments.messageBoxOK("Phrase Loaded", %body);
    }
  }
  else {
    Instruments.messageBoxOK("Error Loading File", %failure);
  }

  $Instruments::GUI::isLoading = false;
}

function InstrumentsClient::onSongLoaded(%this, %song, %filename, %author, %bl_id, %unusedArg, %failure) {
  if (!$Instruments::GUI::isLoading) {
    return;
  }

  if (%failure $= "") {
    if ($Instruments::GUI::isUploading) {
      $Instruments::GUI::Upload::Song = %song;

      InstrumentsClient.saveFile("song", "server", %filename, %author TAB %bl_id);
      $Instruments::GUI::isUploading = false;
      deleteVariables("$Instruments::GUI::Upload::*");
    }
    else if ($Instruments::GUI::isDownloading) {
      $Instruments::GUI::Download::Song = %song;

      InstrumentsClient.saveFile("song", "local", %filename, %author TAB %bl_id);
      $Instruments::GUI::isDownloading = false;
      deleteVariables("$Instruments::GUI::Download::*");
    }
    else {
      InstrumentsClient.textToSong(%song);

      %body = "Successfully loaded " @ %filename @ " by " @ %author;

      if (%bl_id >= 0 && %bl_id != 888888 && %bl_id != 999999) {
         %body = %body @ " (BL_ID: " @ %bl_id @ ")";
      }
      
      InstrumentsClient.setLoadedAuthor("song", %author, %bl_id);
      InstrumentsClient.updateSongOrderList();

      Instruments.messageBoxOK("Song Loaded", %body);
    }
  }
  else {
    Instruments.messageBoxOK("Error Loading File", %failure);
  }

  $Instruments::GUI::isLoading = false;
}

function InstrumentsClient::onSongPhraseData(%this, %index, %phrase, %useServerCmd) {
  if (!$Instruments::GUI::isLoading) {
    return;
  }

  if (%index < 0 || %index > 19) {
    return;
  }

  if ($Instruments::GUI::isUploading) {
    commandToServer('setSongPhrase', %index, %phrase, 1, 1);
  }
  else if ($Instruments::GUI::isDownloading) {
    $Instruments::GUI::Download::SongPhrase[%index] = %phrase;
  }
  else {
    if (%useServerCmd) {
      commandToServer('setSongPhrase', %index, %phrase, 1, 0);
    }

    %phrase = _cleanPhrase(%phrase);
    InstrumentsClient.setSongPhrase(%index, %phrase);
  }
}

function InstrumentsClient::onBindsetLoaded(%this, %filename, %author, %bl_id, %unusedArg, %failure) {
  if (!$Instruments::GUI::isLoading) {
    return;
  }

  if (%failure $= "") {
    if ($Instruments::GUI::isUploading) {
      InstrumentsClient.saveFile("bindset", "server", %filename, %author TAB %bl_id);
      $Instruments::GUI::isUploading = false;
      deleteVariables("$Instruments::GUI::Upload::*");
    }
    else if ($Instruments::GUI::isDownloading) {
      InstrumentsClient.saveFile("bindset", "local", %filename, %author TAB %bl_id);
      $Instruments::GUI::isDownloading = false;
      deleteVariables("$Instruments::GUI::Download::*");
    }
    else {
      %body = "Successfully loaded " @ %filename @ " by " @ %author;

      if (%bl_id >= 0 && %bl_id != 888888 && %bl_id != 999999) {
         %body = %body @ " (BL_ID: " @ %bl_id @ ")";
      }

      InstrumentsClient.setLoadedAuthor("bindset", %author, %bl_id);
      InstrumentsClient.updateSaveButtons();

      Instruments.messageBoxOK("Bindset Loaded", %body);
    }
  }
  else {
    Instruments.messageBoxOK("Error Loading File", %failure);
  }

  $Instruments::GUI::isLoading = false;
}

function InstrumentsClient::onBindsetData(%this, %key, %phraseOrNote, %useServerCmd) {
  if (!$Instruments::GUI::isLoading) {
    return;
  }

  if ($Instruments::GUI::isUploading) {
    commandToServer('Instruments_BindToKey', %key, %phraseOrNote, 1);
  }
  else if ($Instruments::GUI::isDownloading) {
    %index = $Instruments::GUI::Download::NumBinds;

    if (%index $= "") {
      %index = 0;
    }

    $Instruments::GUI::Download::Bind[%index] = %key TAB %phraseOrNote;
    $Instruments::GUI::Download::NumBinds++;
  }
  else {
    %control = InstrumentsMap.keyControl[%key];
    InstrumentsClient.bindToKey(%phraseOrNote, %key, 0, %useServerCmd);
  }
}

function clientCmdInstruments_onFileLoadStart(%type, %filename) {
  InstrumentsClient.onFileLoadStart(%type, %filename, 0);
}

function clientCmdInstruments_onPhraseLoaded(%phrase, %filename, %author, %bl_id, %failure) {
  InstrumentsClient.onPhraseLoaded(%phrase, %filename, %author, %bl_id, "", %failure);
}

function clientCmdInstruments_onSongLoaded(%song, %filename, %author, %bl_id, %failure) {
  InstrumentsClient.onSongLoaded(%song, %filename, %author, %bl_id, "", %failure);
}

function clientCmdInstruments_onSongPhraseData(%index, %phrase) {
  InstrumentsClient.onSongPhraseData(%index, %phrase, 0);
}

function clientCmdInstruments_onBindsetLoaded(%filename, %author, %bl_id, %failure) {
  InstrumentsClient.onBindsetLoaded(%filename, %author, %bl_id, "", %failure);
}

function clientCmdInstruments_onBindsetData(%index, %phrase) {
  InstrumentsClient.onBindsetData(%index, %phrase, 0);
}
