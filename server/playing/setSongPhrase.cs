function serverCmdSetSongPhrase(%client, %index, %phrase, %usingGui, %isUploading) {
  if (!%client.hasInstrumentsClient) {
    return;
  }

  if (!InstrumentsServer.checkSongPermissions(%client, 1)) { 
    return; 
  }

  %index = mRound(%index);

  if (!_isInt(%index)) {
    if (%usingGui) {
      Instruments.messageBoxOK("Developer Error", 
        "Something is wrong with the row index: " @ %index @
        "\n\nPlease send a screenshot of this " @
        "(along with a short explanation of what you did) to Electrk (BL_ID: 12949)", %client);

      return;
    }
    
    messageClient(%client, '', "\n\c6Usage:");
    messageClient(%client, '', "\c3/setSongPhrase phraseNumber(0-19) phrase");
    return;
  }

  if (_strEmpty(%phrase)) {
    if (%usingGui) {
      Instruments.messageBoxOK(%client, "Error", "No phrase to add!", %client);
      return;
    }

    messageClient(%client, '', "\n\c6Usage:");
    messageClient(%client, '', "\c3/setSongPhrase phraseNumber(0-19) phrase");
    return;
  }

  if (strLen(%phrase) > Instruments.const["MAX_PHRASE_LENGTH"]) {
    messageClient(%client, '', "\c6Maximum phrase length is \c3" @ Instruments.const["MAX_PHRASE_LENGTH"] @ " characters");
    return;
  }
  
  if (%index < 0 || %index > 19) {
    messageClient(%client, '', "\c6Valid phrase numbers: \c30-19");
    return;
  }

  if (strPos(%index, ".") != -1) {
    messageClient(%client, '', "\c6Valid phrase numbers: \c30-19");
    messageClient(%client, '', "\c6Whole numbers only (why did you even try this)");
    return;
  } 

  // Believe it or not, this actually worked
  if (striPos(%index, "e") != -1) {
    messageClient(%client, '', "\c6Valid phrase numbers: \c30-19");
    messageClient(%client, '', "\c6Regular notation only (what is wrong with you)");
    return;
  } 

  if (%isUploading) {
    %client.uploadSongPhrase[%index] = %phrase;
  }
  else {
    %client.songPhrase[%index] = %phrase;
  }

  if (!%usingGui) {
    messageClient(%client, '', "\c6Song phrase \c3" @ %index SPC "\c6set to \c3" @ %phrase);
  }

  commandToClient(%client, 'updateSongPhrase', %index, %phrase);
}

function serverCmdInstruments_clearAllSongPhrases(%client) {
  if (!%client.hasInstrumentsClient) {
    return;
  }
  
  if (!InstrumentsServer.checkSongPermissions(%client, 0)) { 
    return; 
  }

  for (%i = 0; %i < 20; %i++) {
    %client.songPhrase[%i] = "";
  }
}
