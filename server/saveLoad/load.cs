function InstrumentsServer::onPhraseLoaded(%this, %phrase, %filename, %author, %bl_id, %client, %failure, %isDownloading) {
  commandToClient(%client, 'Instruments_onPhraseLoaded', %phrase, %filename, %author, %bl_id, %failure);
}

function InstrumentsServer::onSongLoaded(%this, %song, %filename, %author, %bl_id, %client, %failure, %isDownloading) {
  commandToClient(%client, 'Instruments_onSongLoaded', %song, %filename, %author, %bl_id, %failure);
}

function InstrumentsServer::onSongPhraseData(%this, %index, %phrase, %client, %isDownloading) {
  if (_strEmpty(%phrase)) {
    return;
  }

  if (!%isDownloading) {
    %phrase = getSubStr(%phrase, 0, 255);
    %client.songPhrase[%index] = %phrase;
  }
  
  commandToClient(%client, 'Instruments_onSongPhraseData', %index, %phrase, %client);
}

function InstrumentsServer::onBindsetLoaded(%this, %filename, %author, %bl_id, %client, %failure, %isDownloading) {
  commandToClient(%client, 'Instruments_onBindsetLoaded', %filename, %author, %bl_id, %failure);
}

function InstrumentsServer::onBindsetData(%this, %key, %phraseOrNote, %client, %isDownloading) {
  if (_strEmpty(%key)) {
    return;
  }

  if (!%isDownloading && isObject(%client.instrumentBinds)) {
    if (_strEmpty(%phraseOrNote)) {
      %client.instrumentBinds.removeBind(%key);
    }
    else {
      %client.instrumentBinds.addBind(%key, %phraseOrNote);
    }
  }
  
  commandToClient(%client, 'Instruments_onBindsetData', %key, %phraseOrNote);
}

function serverCmdInstruments_LoadFile(%client, %type, %filename, %isDownloading) {
  if (!%client.hasInstrumentsClient) {
    return;
  }

  if (!InstrumentsServer.checkLoadingPermissions(%client, 1)) {
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

  %timeLeft = InstrumentsServer.getTimeLeft(%client.lastInstrumentsLoadTime, $Pref::Server::Instruments::LoadingTimeout * 1000);

  if (!_strEmpty(%client.lastInstrumentsLoadTime) && %timeLeft > 0) {
    Instruments.messageBoxOK("Loading Timeout", 
      "Please wait " @ mCeil(%timeLeft / 1000) @ " second(s) before attempting to load again.", %client);
    return;
  }

  Instruments.loadFile(%type, %filename, %client, %isDownloading);
}
