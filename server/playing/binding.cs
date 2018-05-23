function serverCmdInstruments_BindToKey(%client, %key, %phraseOrNote, %isUploading) {
  if (!%client.hasInstrumentsClient) {
    return;
  }

  if (!InstrumentsServer.checkBindsetPermissions(%client, 0)) {
    return;
  }

  // if (%client.lastInstrumentsBindTime !$= "" && getSimTime() - %client.lastInstrumentsBindTime < 20) {
  //    return;
  // }

  if (_strEmpty(%key)) {
    return;
  }

  // %client.lastInstrumentsBindTime = getSimTime();

  if (!isObject(%client.instrumentBinds)) {
    %client.instrumentBinds = new ScriptObject() {
      class = "InstrumentsBindset";
      client = %client;
    };
  }

  if (!isObject(%client.instrumentUploadBinds)) {
    %client.instrumentUploadBinds = new ScriptObject() {
      class = "InstrumentsBindset";
      client = %client;
    };
  }

  if (%isUploading) {
    %binds = %client.instrumentUploadBinds;
  }
  else {
    %binds = %client.instrumentBinds;
  }

  if (_strEmpty(%phraseOrNote)) {
    %binds.removeBind(%key);
  }
  else {
    %binds.addBind(%key, %phraseOrNote);
  }
}

function serverCmdInstruments_clearAllKeys(%client) {
  if (!%client.hasInstrumentsClient) {
    return;
  }

  if (!InstrumentsServer.checkBindsetPermissions(%client, 0)) {
    return;
  }

  if (!isObject(%client.instrumentBinds)) {
    %client.instrumentBinds = new ScriptObject() {
      class = "InstrumentsBindset";
      client = %client;
    };
  }
  else {
    %client.instrumentBinds.clearBinds();
  }
}
