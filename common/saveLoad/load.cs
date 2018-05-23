function Instruments::loadFile(%this, %type, %filename, %client, %isDownloading) {
  %filename = stripTrailingSpaces(%filename);

  if (_strEmpty(%filename)) {
    Instruments.messageBoxOK("Error", "No filename.", %client);
    return;
  }

  if (!Instruments.validateFilename(%filename, %client, 1)) { 
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

  if (%isDownloading $= "") {
    %isDownloading = false;
  }

  %path = Instruments.getFilePath(%type, %filename, %localOrServer);

  if (!isFile(%path)) {
    Instruments.messageBoxOK("Error", "File does not exist.", %client);
    return;
  }

  if (%localOrServer $= "server") {
    if (%type $= "bindset") {
      %client.instrumentBinds.clearBinds();
    }

    commandToClient(%client, 'Instruments_onFileLoadStart', %type, %filename);
  }
  else if (%localOrServer $= "local") {
    InstrumentsClient.onFileLoadStart(%type, %filename, 1);
  }

  %file = new FileObject();
  %file.openForRead(%path);

  %phrase = "";
  %song = "";
  %songPhraseCount = 0;
  %bindCount = 0;
  %lineCount = 0;
  %author = "";
  %id = -1;

  %failure = "";

  while (!%file.isEoF()) {
    // Hard-coded to prevent people from cheating the song phrase limit (20) and bind limit (84)
    if (%songPhraseCount >= 20 || %bindCount >= 84) { 
      break; 
    }

    %line = %file.readLine();

    if (%lineCount == 0) {
      // File was saved when the instruments mod did not even keep track of its version number
      // It probably will not work
      if (getFieldCount(%line) <= 1) {
        %failure = "This file was saved using an extremely old version of the instruments mod and" SPC 
                   "will probably not work properly.";
        break;
      }
      else {
        %notation = getField(%line, 1);

        if (%notation !$= $Instruments::NotationVersion) {
          %failure = "This file was saved using a different version of the instruments notation and" SPC 
                     "will probably not work properly.";
          break;
        }
      }
    }
    else if (%lineCount == 1) {
      %author = getField(%line, 0);
      %id = getField(%line, 1);
    }
    else {
      if (%type $= "phrase") {
        %phrase = _cleanPhrase(%line);
        break;
      }
      else if (%type $= "song") {
        %line = _cleanPhrase(%line);
        
        if (%lineCount == 2) {
          %song = %line;
        }
        else if (%lineCount > 2) {
          Instruments.onFileData(%type, %songPhraseCount TAB %line, %client, %isDownloading);
          %songPhraseCount++;
        }
      }
      else if (%type $= "bindset") {
        %key = getField(%line, 0);
        %phrase = getField(%line, 1);

        Instruments.onFileData(%type, %key TAB %phrase, %client, %isDownloading);
        %bindCount++;
      }
    }

    %lineCount++;
  }

  %file.close();
  %file.delete();

  if (%type $= "phrase") {
    Instruments.onFileLoaded(%type, %phrase, %filename, %author, %id, %client, %failure, %isDownloading);
  }

  if (%type $= "song") {
    Instruments.onFileLoaded(%type, %song, %filename, %author, %id, %client, %failure, %isDownloading);
  }

  if (%type $= "bindset") {
    Instruments.onFileLoaded(%type, "", %filename, %author, %id, %client, %failure, %isDownloading);
  }

  if (%client !$= "") {
    %client.lastInstrumentsLoadTime = getSimTime();
  }
}

function Instruments::onFileLoaded(%this, %type, %data, %filename, %author, %bl_id, %client, %failure, %isDownloading) {
  if (%client $= "") {
    %namespace = "InstrumentsClient";
  }
  else {
    %namespace = "InstrumentsServer";
  }

  if (%type $= "phrase") {
    %namespace.onPhraseLoaded(%data, %filename, %author, %bl_id, %client, %failure, %isDownloading);
  }
  else if (%type $= "song") {
    %namespace.onSongLoaded(%data, %filename, %author, %bl_id, %client, %failure, %isDownloading);
  }
  else if (%type $= "bindset") {
    %namespace.onBindsetLoaded(%filename, %author, %bl_id, %client, %failure, %isDownloading);
  }
}

function Instruments::onFileData(%this, %type, %data, %client) {
  %field1 = getField(%data, 0);
  %field2 = getField(%data, 1);

  if (%type $= "song") {
    if (%client $= "") {
      InstrumentsClient.onSongPhraseData(%field1, %field2, 1);
    }
    else {
      InstrumentsServer.onSongPhraseData(%field1, %field2, %client, %isDownloading);
    }
  }
  else if (%type $= "bindset") {
    if (%client $= "") {
      InstrumentsClient.onBindsetData(%field1, %field2, 1);
    }
    else {
      InstrumentsServer.onBindsetData(%field1, %field2, %client, %isDownloading);
    }
  }
}
