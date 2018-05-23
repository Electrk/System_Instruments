function InstrumentsServer::getTimeLeft(%this, %lastTime, %timeout) {
  return %timeout - (getSimTime() - %lastTime);
}

function serverCmdInstruments_GetVersion(%client, %version, %notationVersion) {
  if (%client.hasInstrumentsClient) {
    return;
  }
  
  if (%version $= "" || %notationVersion $= "") {
    Instruments.messageBoxOK("Failure", "You did it wrong.", %client);
    return;
  }

  %serverVersion = strReplace($Instruments::Version, ".", "\t");
  %clientVersion = strReplace(%version, ".", "\t");

  if (getFieldCount(%serverVersion) != 3) {
    error("serverCmdInstruments_GetVersion(" @ %client @ ") - Malformed server version: " @ %serverVersion);
    messageClient(%client, '', "Malformed server version: " @ %serverVersion);
    return;
  }
  else if (getFieldCount(%clientVersion) != 3) {
    messageClient(%client, '', "Malformed client version: " @ %clientVersion);
    return;
  }

  %client.canUseInstruments = true;
  %client.hasInstrumentsClient = true;
  %client.instrumentsVersion = %version;
  %client.instrumentsNotationVersion = %notationVersion;

  // Versions are incompatible and cannot be used, period.
  if (getField(%clientVersion, 0) != getField(%serverVersion, 0)) {
    %client.canUseInstruments = false;
  }

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
}

function serverCmdInstruments_CanIUse(%client, %type, %showMessage) {
  if (!%client.hasInstrumentsClient) {
    return;
  }

  if (%client.instrumentsCanUseTimeout !$= "" && getSimTime() - %client.instrumentsCanUseTimeout < 100) {
    return;
  }

  %client.instrumentsCanUseTimeout = getSimTime();

  if (!%client.hasSpawnedOnce) {
    commandToClient(%client, 'Instruments_CanIUse', "instruments", 0);
    Instruments.messageBoxOK("Please wait", "You have not spawned yet.", %client);
    return;
  }

  if (_strEmpty(%showMessage)) {
    %showMessage = true;
  }
  
  %canUse = InstrumentsServer.checkInstrumentPermissions(%client, 1);
  commandToClient(%client, 'Instruments_CanIUse', "instruments", %canUse);

  if (!%canUse || _strEmpty(%type) || %type $= "instruments") {
    return;
  }

  if (%type $= "all") {
    %showMessage = false;
  }

  if (%type $= "songs" || %type $= "all") {
    %canUse = InstrumentsServer.checkSongPermissions(%client, %showMessage);
    commandToClient(%client, 'Instruments_CanIUse', "songs", %canUse);
  }
  
  if (%type $= "bindsets" || %type $= "all") {
    %canUse = InstrumentsServer.checkBindsetPermissions(%client, %showMessage);
    commandToClient(%client, 'Instruments_CanIUse', "bindsets", %canUse);
  }
  
  if (%type $= "saving" || %type $= "all") {
    %canUse = InstrumentsServer.checkSavingPermissions(%client, %showMessage);
    commandToClient(%client, 'Instruments_CanIUse', "saving", %canUse);
  }
  
  if (%type $= "loading" || %type $= "all") {
    %canUse = InstrumentsServer.checkLoadingPermissions(%client, %showMessage);
    commandToClient(%client, 'Instruments_CanIUse', "loading", %canUse);
  }
  
  if (%type $= "deleting" || %type $= "all") {
    %canUse = InstrumentsServer.checkDeletingPermissions(%client, %showMessage);
    commandToClient(%client, 'Instruments_CanIUse', "deleting", %canUse);
  }
}
