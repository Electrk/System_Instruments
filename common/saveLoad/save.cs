// mod version TAB notation version
// player name TAB bl_id
// phrase (e.g. C3,C4,C5)

// mod version TAB notation version
// player name TAB bl_id
// song (e.g. 0,1,2)
// phrase 0
// phrase 1
// phrase 2
// etc.


function Instruments::saveFile(%this, %type, %filename, %phraseOrSong, %client, %overwrite, %authorToWrite, %isUploading) {
  if (%type !$= "phrase" && %type !$= "song" && %type !$= "bindset") { 
    return; 
  }

  %filename = stripTrailingSpaces(%filename);
  %phraseOrSong = _cleanPhrase(%phraseOrSong);

  if (!Instruments.validateFilename(%filename, %client, 1)) { 
    return; 
  }

  if (%authorToWrite $= "") {
    if (%client $= "") {
      %authorToWrite = $Pref::Player::NetName TAB getNumKeyID();
    }
    else {
      %authorToWrite = %client.getPlayerName() TAB %client.getBLID();
    }
  }

  if (!Instruments.validateFileAuthor(%authorToWrite, %client, 1)) {
    return;
  }

  if (%client $= "") {
    %binds = InstrumentsClient.binds;
  }
  else {
    if (%isUploading) {
      %binds = %client.instrumentUploadBinds;
    }
    else {
      %binds = %client.instrumentBinds;
    }
  }

  if (%type $= "bindset" && %binds.bindCount < 3) {
    return;
  }

  if (%client !$= "") {
    if (!isObject(%client)) {
      return;
    }

    %localOrServer = "server";
  }
  else {
    %localOrServer = "local";
  }

  if (%isUploading $= "") {
    %isUploading = false;
  }

  %path = Instruments.getFilePath(%type, %filename, %localOrServer);
  %overwritingFile = isFile(%path);

  if (%overwritingFile) {

    // If we're saving to server, we need to check if client has permission on the serverto overwrite files
    // Otherwise, if it were on the client's local system, you don't need to check for permissions (obviously)
    if (%client !$= "") {
      %hasPermission = InstrumentsServer.checkDeletingPermissions(%client, 0);

      if (!%hasPermission) {
        Instruments.messageBoxOK("File Exists", "File already exists!", %client);
        return;
      }

      %author = Instruments.getFileAuthor(%type, %filename, %localOrServer);
      %bl_id = getField(%author, 1);

      if (%bl_id != %client.getBLID() && !%client.isAdmin && !%client.isSuperAdmin && !%client.isHost()) {
        Instruments.messageBoxOK("File Exists", "File already exists!", %client);
        return;
      }
    }

    if (!%overwrite) {

      if (%client $= "") {
        // If we're saving to local
        %yes = "Instruments.saveFile(\"" @ %type @ "\", \"" @ %filename @ "\", \"" @ %phraseOrSong @ "\", \"\", 1);";
        Instruments.messageBoxYesNo("File Exists", "File already exists!  Overwrite?", %yes);
        return;
      }

      if (%type $= "phrase") {
        %client.instrumentPhrase = %phraseOrSong;
        %yes = 'savePhraseOverwrite';
      }
      
      if (%type $= "song") {
        %client.instrumentSong = %phraseOrSong;
        %yes = 'saveSongOverwrite';
      }
      
      if (%type $= "bindset") {
        %yes = 'saveBindsetOverwrite';
      }

      %client.instrumentFileAuthor = %authorToWrite;
      %client.instrumentFileName = %filename;

      %author = Instruments.getFileAuthor(%type, %filename, %localOrServer);
      %name = getField(%author, 0);
      %bl_id = getField(%author, 1);

      %body = "File already exists!\n\n<color:000000>Author: <color:0000FF>" @ %name @ 
        "\n<color:000000>BL_ID: <color:0000FF>" @ %BL_ID @ "\n\n<color:000000>Overwrite?";
        
      Instruments.messageBoxYesNo("File Exists", %body, %yes, %client);
      return;
    }
  }

  if (%client !$= "") {
    if (!isObject(%client)) {
      return;
    }

    %localOrServer = "server";
  }
  else {
    %localOrServer = "local";
  }

  Instruments.onFileSaveStart(%type, %filename, %phraseOrSong, %client, %isUploading);

  %file = new FileObject();
  %file.openForWrite(%path);
  %file.writeLine($Instruments::Version TAB $Instruments::NotationVersion);
  %file.writeLine(%authorToWrite);

  if (%type !$= "bindset") {
    %file.writeLine(%phraseOrSong);
  }

  if (%type $= "song") {
    for (%i = 0; %i < 20; %i++) {

      if (%client $= "") {
        if ($Instruments::GUI::isDownloading) {
          %phrase = $Instruments::GUI::Download::SongPhrase[%i];
        }
        else {
          %phrase = getField(InstrumentsDlg_SongPhraseList.getRowText(%i), 1);
        }
      }
      else {
        if (%isUploading) {
          %phrase = %client.uploadSongPhrase[%i];
          %client.uploadSongPhrase[%i] = "";
        }
        else {
          %phrase = %client.songPhrase[%i];
        }
      }

      if (_strEmpty(%phrase)) { 
        break; 
      }

      %file.writeLine(%phrase);
    }
  }
  else if (%type $= "bindset") {
    if ($Instruments::GUI::isDownloading) {
      %bindCount = $Instruments::GUI::Download::NumBinds;
    }
    else {
      %bindCount = %binds.currIndex;
    }

    for (%i = 0; %i < %bindCount; %i++) {
      
      // Hard-coded
      if (%i >= 84) {
        break;
      }

      if ($Instruments::GUI::isDownloading) {
        %bind = $Instruments::GUI::Download::Bind[%i];
      }
      else {
        %bind = %binds._bind[%i];
      }

      if (_strEmpty(%bind)) {
        continue;
      }

      %file.writeLine(%bind);
    }
  }

  %file.close();
  %file.delete();

  if (%client !$= "") {
    %client.lastInstrumentsSaveTime = getSimTime();
  }

  if (!isFile(%path)) {
    %failure = "Could not save file!";
  }

  Instruments.onFileSaved(%type, %filename, %client, %overwritingFile, %authorToWrite, %failure, %isUploading);
}

function Instruments::onFileSaved(%this, %type, %filename, %client, %overwriting, %author, %failure, %isUploading) {
  if (%client $= "") {
    %namespace = "InstrumentsClient";
  }
  else {
    %namespace = "InstrumentsServer";
  }

  %authorName = getField(%author, 0);
  %authorBL_ID = getField(%author, 1);

  if (%failure $= "") {

    if (!%overwriting) {
      if (%client $= "") {
        InstrumentsClient.refreshFileLists();
      }
      else if (isFile(%path)) {
        commandToAll('Instruments_onFileAdded', %filename, %type, %author);
      }
    }

    if (%type $= "phrase") {
      %namespace.onPhraseSaved(%filename, %authorName, %authorBL_ID, %client, %isUploading);
    }
    else if (%type $= "song") {
      %namespace.onSongSaved(%filename, %authorName, %authorBL_ID, %client, %isUploading);
    }
    else if (%type $= "bindset") {
      %namespace.onBindsetSaved(%filename, %authorName, %authorBL_ID, %client, %isUploading);
    }
  }
  else {
    %namespace.onFileSaveError(%type, %filename, %client, %failure, %isUploading);
  }
}

function Instruments::onFileSaveStart(%this, %type, %filename, %phraseOrSong, %client, %isUploading) {
  if (%client $= "") {
    %namespace = "InstrumentsClient";
  }
  else {
    %namespace = "InstrumentsServer";
  }

  %namespace.onFileSaveStart(%type, %filename, %phraseOrSong, %client, %isUploading);
}
