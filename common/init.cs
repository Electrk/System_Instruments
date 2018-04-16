$Instruments::Version = "1.0.1";
$Instruments::NotationVersion = "3";

exec("./classes/InstrumentData.cs");
exec("./classes/InstrumentsNamespace.cs");
exec("./classes/InstrumentsBindset.cs");

if ($Pref::Instruments::SampleFilesCopied $= "") {
  $Pref::Instruments::SampleFilesCopied = false;
}

if (!isObject(Instruments)) {

  // Shared between client and server

  new ScriptObject(Instruments) {
    class = "InstrumentsNamespace";

    // Constants are shared by both client and server code
    // They're just here to make it easier for me in case I want to change something later
    // Don't think that changing any of these will allow you do get around any server-side limitations
    // They won't.

    // Don't change these unless you want to fuck something up

    const["HIGHEST_DELAY"] = 5000;
    const["LOWEST_DELAY"] = 50;
    const["DEFAULT_DELAY"] = 500;
    const["HIGHEST_TEMPO"] = mFloor(60000 / 50);
    const["LOWEST_TEMPO"] = mFloor(60000 / 5000);
    const["DEFAULT_TEMPO"] = 120;
    const["MAX_PHRASE_LENGTH"] = 255;
    const["MAX_SONG_PHRASES"] = 20;
    const["SONG_ORDER_LIMIT"] = 120;
    const["NUM_KEY_ROWS"] = 6;
    const["NUM_KEY_COLUMNS"] = 14;
    const["DISALLOWED_KEYS"] = "escape\tf1\tf10\tf11\tf12\tNUMLOCK\tNUMPADMULT\t`\t~\tCAPSLOCK\tSCROLLLOCK" TAB
                                "insert\thome\tpageup\tdelete\tend\tpagedown";
  };
}

function Instruments::isKeyAllowed(%this, %key) {
  if (getWordCount(%key) > 1 || getWordCount(%key) < 1) {
    return false;
  }

  %disallowed = Instruments.const["DISALLOWED_KEYS"];
  %count = getFieldCount(%disallowed);

  for (%i = 0; %i < %count; %i++) {
    %disallowedKey = getField(%disallowed, %i); 

    if (%key $= %disallowedKey) {
      return false;
    }
  }

  return true;
}

function Instruments::copySampleFiles(%this, %type) {
  %type = strLwr(%type);

  if (%type $= "bindset") {
    %path = "Add-Ons/System_Instruments/common/files/sampleBindsets/*.txt";
  }
  else if (%type $= "phrase") {
    %path = "Add-Ons/System_Instruments/common/files/samplePhrases/*.txt";
  }
  else if (%type $= "song") {
    %path = "Add-Ons/System_Instruments/common/files/sampleSongs/*.txt";
  }
  else {
    return;
  }

  %folderPath = "instruments/" @ %type @ "s/";

  createPath("config/client/" @ %folderPath);
  createPath("config/server/" @ %folderPath);


  for (%file = findFirstFile(%path); %file !$= ""; %file = findNextFile(%path)) {
    %clientPath = "config/client/" @ %folderPath @ fileName(%file);
    %serverPath = "config/server/" @ %folderPath @ fileName(%file);

    if (!isFile(%clientPath)) {
      fileCopy(%file, %clientPath);
    }

    if (!isFile(%serverPath)) {
      fileCopy(%file, %serverPath);
    }
  }

  $Pref::Instruments::SampleFilesCopied = true;
}

if (!$Pref::Instruments::SampleFilesCopied) {
  Instruments.copySampleFiles("bindset");
  Instruments.copySampleFiles("phrase");
  Instruments.copySampleFiles("song");
}


function Instruments::init(%this) {
  // Magic numbers
  // Don't change these
  %notePitches = "1\t1.06\t1.12\t1.19\t0.945\t1\t1.06\t1.12\t1.19\t1.2625\t0.8925\t0.945";
  // There doesn't seem to be a consistent pattern so it was pretty much trial and error

  %noteIntervals = "C\tC\tC\tC\tF\tF\tF\tF\tF\tF\tC\tC";
  $Instruments::Notes = "C\tCS\tD\tDS\tE\tF\tFS\tG\tGS\tA\tAS\tB";
  %notes = $Instruments::Notes;

  %length = getFieldCount(%noteIntervals);

  for (%n = 0; %n < %length; %n++) {
    // Used to convert melodic notes to "interval" note (actual C or F sound file of corresponding octave)
    $Instruments::NoteToInterval[getField(%notes, %n)] = getField(%noteIntervals, %n);
    // Used to determine how much said note will be pitched up or down
    $Instruments::NoteToPitch[getField(%notes, %n)] = getField(%notePitches, %n);
  }
}

Instruments.init();