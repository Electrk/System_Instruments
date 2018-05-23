function InstrumentsServer::onFileSaveStart(%this, %type, %filename, %phraseOrSong, %client, %isUploading) {
  commandToClient(%client, 'Instruments_onFileSaveStart', %type, %filename);
}

function InstrumentsServer::onPhraseSaved(%this, %filename, %author, %bl_id, %client, %isUploading) {
  commandToClient(%client, 'Instruments_onPhraseSaved', %filename, %author, %bl_id);
}

function InstrumentsServer::onSongSaved(%this, %filename, %author, %bl_id, %client, %isUploading) {
  commandToClient(%client, 'Instruments_onSongSaved', %filename, %author, %bl_id);
}

function InstrumentsServer::onBindsetSaved(%this, %filename, %author, %bl_id, %client, %isUploading) {
  if (%isUploading) {
    %client.instrumentUploadBinds.clearBinds();
  }

  commandToClient(%client, 'Instruments_onBindsetSaved', %filename, %author, %bl_id);
}

function InstrumentsServer::onFileSaveError(%this, %type, %filename, %client, %failure, %isUploading) {
  commandToClient(%client, 'Instruments_onFileSaveError', %type, %filename, %failure);
}

function serverCmdInstruments_SaveFile(%client, %type, %filename, %phraseOrSong, %overwrite, %author, %isUploading) {
  if (!%client.hasInstrumentsClient) {
    return;
  }

  if (!InstrumentsServer.checkSavingPermissions(%client, 1)) {
    return;
  }

  if (_strEmpty(%filename) || _strEmpty(%type)) {
    return;
  }

  if (%type $= "song" && !InstrumentsServer.checkSongPermissions(%client, 1)) { 
    return; 
  }

  if (%type $= "bindset" && !InstrumentsServer.checkBindsetPermissions(%client, 1)) { 
    return; 
  }

  if (%type $= "phrase" || %type $= "song") {
    if (_strEmpty(%phraseOrSong)) {
      Instruments.messageBoxOK("Error", "No" SPC %type SPC "to save!", %client);
      return;
    }
  }

  if (%type $= "bindset") {
    if (%client.instrumentBinds.bindCount <= 0) {
      Instruments.messageBoxOK("Error", "No bindset to save!", %client);
      return;
    }

    if (%client.instrumentBinds.bindCount < 3) {
      Instruments.messageBoxOK("Error", "You need at least 3 keybinds before you can save.", %client);
      return;
    }
  }

  %timeLeft = InstrumentsServer.getTimeLeft(%client.lastInstrumentsSaveTime, $Pref::Server::Instruments::SavingTimeout * 1000);

  if (!_strEmpty(%client.lastInstrumentsSaveTime) && %timeLeft > 0) {
    Instruments.messageBoxOK("Saving Timeout", 
      "Please wait " @ mCeil(%timeLeft / 1000) @ " second(s) before attempting to save again.", %client);
    return;
  }

  Instruments.saveFile(%type, %filename, %phraseOrSong, %client, %overwrite, %author, %isUploading);
}

function serverCmdSaveSongOverwrite(%client) {
  %isUploading = %client.instrumentIsUploading;
  %author = %client.instrumentFileAuthor;
  %filename = %client.instrumentFileName;
  %song = %client.instrumentSong;
  
  serverCmdInstruments_SaveFile(%client, "song", %filename, %song, 1, %author, %isUploading);
}

function serverCmdSavePhraseOverwrite(%client) {
  %isUploading = %client.instrumentIsUploading;
  %author = %client.instrumentFileAuthor;
  %filename = %client.instrumentFileName;
  %phrase = %client.instrumentPhrase;

  serverCmdInstruments_SaveFile(%client, "phrase", %filename, %phrase, 1, %author, %isUploading);
}

function serverCmdSaveBindsetOverwrite(%client) {
  %isUploading = %client.instrumentIsUploading;
  %author = %client.instrumentFileAuthor;
  %filename = %client.instrumentFileName;
  
  serverCmdInstruments_SaveFile(%client, "bindset", %filename, "", 1, %author, %isUploading);
}
