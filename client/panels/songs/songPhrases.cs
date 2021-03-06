function clientCmdUpdateSongPhrase(%index, %phrase) {
  if (%index < 0 || %index >= $Instruments::Client::ServerPref::MaxSongPhrases) {
    return;
  }

  %count = InstrumentsDlg_SongPhraseList.rowCount();

  if (%index > %count) {
    return;
  }

  InstrumentsClient.setSongPhrase(%index, %phrase);
}

function InstrumentsClient::clickSetSongPhrase(%this, %phrase) {
  %index = InstrumentsDlg_SongPhraseList.getSelectedId();
  commandToServer('setSongPhrase', %index, %phrase, 1);
}

function InstrumentsClient::clickEditSongPhrase(%this) {
  %id = InstrumentsDlg_SongPhraseList.getSelectedId();
  %phrase = getField(InstrumentsDlg_SongPhraseList.getRowTextById(%id), 1);

  $Instruments::GUI::SongPhrase = _cleanPhrase(%phrase);

  %title = "Edit Song Phrase";
  %label = "Phrase";
  %editVar = "$Instruments::GUI::SongPhrase";
  %editCmd = "";
  %btnText = "Done";
  %btnCmd = "InstrumentsClient.clickSetSongPhrase(InstrumentsEditTextDlg_TextEdit.getValue());";
  %footer = "";

  InstrumentsClient.openEditTextDialog(%title, %label, %editVar, %editCmd, %btnText, %btnCmd, %footer);
}

function InstrumentsClient::selectSongPhrase(%this) {
  // callback
}

function InstrumentsClient::addSongPhrase(%this, %phrase) {
  %count = InstrumentsDlg_SongPhraseList.rowCount();

  if (%count >= $Instruments::Client::ServerPref::MaxSongPhrases) {
    return;
  }

  InstrumentsDlg_SongPhraseList.addRow(%count, (%count + 1) @ "." TAB %phrase);
}

function InstrumentsClient::removeSongPhrase(%this, %index) {
  %count = InstrumentsDlg_SongPhraseList.rowCount();

  if (%index < 0 || %index >= %count) {
    return;
  }

  InstrumentsDlg_SongPhraseList.removeRow(%index);
  InstrumentsClient.updateSongOrderList();
}

function InstrumentsClient::setSongPhrase(%this, %index, %phrase) {
  %phrase = getSubStr(%phrase, 0, 255);
  
  InstrumentsDlg_SongPhraseList.setRowById(%index, (%index + 1) @ "." TAB %phrase);
  InstrumentsClient.updateSongOrderList();
}

function InstrumentsClient::clearSongPhrase(%this, %index) {
  if (%index $= "") {
    %index = InstrumentsDlg_SongPhraseList.getSelectedRow();
  }

  InstrumentsClient.setSongPhrase(%index, "");
}

function InstrumentsClient::clearAllSongPhrases(%this, %useServerCmd) {
  if (%useServerCmd) {
    commandToServer('Instruments_clearAllSongPhrases');
  }
  
  InstrumentsClient.clearSong();
  
  for (%i = 0; %i < $Instruments::Client::ServerPref::MaxSongPhrases; %i++) {
    InstrumentsClient.clearSongPhrase(%i);
  }

  InstrumentsClient.setLoadedAuthor("song", "", "");
}
