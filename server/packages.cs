package System_Instruments__server {

  function GameConnection::loadMission(%this) {
    Parent::loadMission(%this);
    InstrumentsServer.schedule(1, sendFilenames, %this, "all");
  }

  function GameConnection::autoAdminCheck(%this) {
    Parent::autoAdminCheck(%this);
    commandToClient(%this, 'Instruments_GetVersion', $Instruments::Version, $Instruments::NotationVersion);
    InstrumentsServer.schedule(1, sendInstrumentList, %this);
  }

  function GameConnection::onDeath(%client, %killerPlayer, %killerClient, %damageType, %damageLocation) {
    if (isObject(%player = %client.player) && %player.isPlayingInstrument) {
      InstrumentsServer.stopPlaying(%player);
    }

    Parent::onDeath(%client, %killerPlayer, %killerClient, %damageType, %damageLocation);
  }

  function GameConnection::onDrop(%client, %reason) {
    if (isObject(%client.instrumentBinds)) {
      %client.instrumentBinds.delete();
    }

    if (isObject(%client.instrumentUploadBinds)) {
      %client.instrumentUploadBinds.delete();
    }

    Parent::onDrop(%client, %reason);
  }

  function WeaponImage::onMount(%this, %obj, %slot) {
    Parent::onMount(%this, %obj, %slot);

    if (%this.instrumentType !$= "") {
      %obj.setInstrument(%this.instrumentType);
    }
  }

  function WeaponImage::onUnmount(%this, %obj, %slot) {
    Parent::onUnmount(%this, %obj, %slot);

    if (%obj.instrument !$= "") {
      %obj.setInstrument("");
    }
  }
};
activatePackage(System_Instruments__server);
