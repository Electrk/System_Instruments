function InstrumentsClient::downloadSelectedFile(%this) {
  if (InstrumentsDlg_LocalFileList.getSelectedId() >= 0) {
    return;
  }

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

  InstrumentsClient.downloadFile(%type, InstrumentsClient.getSelectedFile());
}

function InstrumentsClient::downloadFile(%this, %type, %filename) {
  $Instruments::GUI::isDownloading = true;
  InstrumentsClient.loadFile(%type, %filename, "server");
}

function InstrumentsClient::uploadSelectedFile(%this) {
  if (InstrumentsDlg_ServerFileList.getSelectedId() >= 0) {
    return;
  }

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

  InstrumentsClient.uploadFile(%type, InstrumentsClient.getSelectedFile());
}

function InstrumentsClient::uploadFile(%this, %type, %filename) {
  $Instruments::GUI::isUploading = true;
  InstrumentsClient.loadFile(%type, %filename, "local");
}
