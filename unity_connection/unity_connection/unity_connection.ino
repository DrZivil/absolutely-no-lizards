#include <SerialCommand.h>
const int tvRemote = 2;
const int tvRemoteToUnity = 8;
const int leaverPin = 6;
const int moneyReaderPin = 5;
const int piezoBuzzer = 7;
const int toneLength = 250;

const int chargeEnergy = 3;
const int chargeEnergyUnity = 18;

const int fakeWiiPin = 12;
const int bringDownWii = 17;

int val;

// flags
bool leaverTriggered = false;
bool tvRemoteDown = false;

int freqsUp[] = {440, 660};
int freqsDown[] = {660, 440};
int freqsGameOver[] = {440,440,440,660};

int gameOverRythm[] = {240,120,120,480};

SerialCommand sCmd;

int key = 0;

void setup() {
  Serial.begin(9600);

  pinMode(leaverPin, INPUT);
  pinMode(moneyReaderPin, INPUT);

  pinMode(piezoBuzzer, OUTPUT);

  // Pin Mode for Light Bulbs
  pinMode(8, OUTPUT);
  pinMode(9, OUTPUT);
  pinMode(10, OUTPUT);
  pinMode(11, OUTPUT);

  pinMode(fakeWiiPin, INPUT);
  pinMode(chargeEnergy, INPUT);

  digitalWrite(leaverPin, HIGH);
  digitalWrite(moneyReaderPin, LOW);
  digitalWrite(piezoBuzzer, LOW);

  sCmd.addCommand("buzz_up", buzzUpHandler);
  sCmd.addCommand("buzz_down", buzzDownHandler);
  sCmd.addCommand("0", ledOneOn);
  sCmd.addCommand("1", ledTwoOn);
  sCmd.addCommand("2", ledThreeOn);
  sCmd.addCommand("3", ledFourOn);
  sCmd.addCommand("4", ledOneOff);
  sCmd.addCommand("5", ledTwoOff);
  sCmd.addCommand("6", ledThreeOff);
  sCmd.addCommand("7", ledFourOff);
}

void ledOneOn() {
  digitalWrite(8, HIGH);
}
void ledTwoOn() {
  digitalWrite(9, HIGH);
}
void ledThreeOn() {
  digitalWrite(10, HIGH);
}
void ledFourOn() {
  digitalWrite(11, HIGH);
}

void ledOneOff() {
  digitalWrite(8, LOW);
}
void ledTwoOff() {
  digitalWrite(9, LOW);
}
void ledThreeOff() {
  digitalWrite(10, LOW);
}
void ledFourOff() {
  digitalWrite(11, LOW);
}

// "Melody" for bar up
void buzzUpHandler() {
  for (int i = 0; i < 2; i ++) {
    tone(piezoBuzzer, freqsUp[i], toneLength);
    delay(toneLength);
  }
}
// "Meldoy" for bar down
void buzzDownHandler() {
  for (int i = 0; i < 2; i ++) {
    tone(piezoBuzzer, freqsDown[i], toneLength);
    delay(toneLength);
  }
}

// "Meldoy" for game over
void buzzGameOverHandler() {
  for (int i = 0; i < 4; i ++) {
    tone(piezoBuzzer, freqsGameOver[i], toneLength);
    delay(toneLength);
  }
}

/**
   Serial Send Mapping:

    Arduino -> Unity
    - Remote TV Arduino -> Unity: int 8
    - Leaver Arduino -> Unity: int 6
    - money reader Arduino -> Unity: int 5;
    - charge Energy Button -> Unity: int 18

    Unity -> Arduino
    - piezoBuzzer -> int 7;
    - Light Bulbs pins -> 8 - 11

    - Send wheel commands for bar selection:
      bar 1 -> int 10
      bar 2 -> int 20
      bar 3 -> int 30
      bar 4 -> int 40

*/
void loop() {
  val = analogRead(A0);
  potiMapping(val);

  if (Serial.available() > 0) {
    sCmd.readSerial();
  }
  if (digitalRead(leaverPin) == LOW && leaverTriggered == false) {
    delay(10);
    if (digitalRead(leaverPin) == LOW && leaverTriggered == false) {
      //Serial.println("leaver low");
      leaverTriggered = true;
      Serial.write(3);
      Serial.flush();
      delay(20);
    }
  }
  // Reset Leaver Function
  if (digitalRead(leaverPin) == HIGH && leaverTriggered == true) {
    delay(10);
    if (digitalRead(leaverPin) == HIGH && leaverTriggered == true) {
      //Serial.println("leaver high");
      leaverTriggered = false;
    }
  }
  Serial.println(digitalRead(moneyReaderPin));
  if (digitalRead(moneyReaderPin) == 1) {
    delay(10);
    if (digitalRead(moneyReaderPin) == 1) {

      Serial.write(5);
      Serial.flush();
      delay(20);
    }
  }


  if (digitalRead(fakeWiiPin) == HIGH) {
    delay(10);
    if (digitalRead(fakeWiiPin) == HIGH) {
      Serial.write(bringDownWii);
      Serial.flush();
      delay(20);
    }
  }

  if (digitalRead(chargeEnergy) == 1) {
    delay(10);
    if (digitalRead(chargeEnergy) == 1) {
      Serial.write(chargeEnergyUnity);
      Serial.flush();
      delay(20);
    }
  }

  if (digitalRead(tvRemote) == 1 && tvRemoteDown == false) {
    delay(10);
    if (digitalRead(tvRemote) == 1 && tvRemoteDown == false) {
      tvRemoteDown = true;
      //      Serial.println("state in 1: " + tvRemoteDown);
      Serial.write(tvRemoteToUnity);
      Serial.flush();
      delay(20);
    }
  }
  if (digitalRead(tvRemote) == 0 && tvRemoteDown == true) {
    //    Serial.println("state in 0: " + tvRemoteDown);
    tvRemoteDown = false;
  }
}

void potiMapping(int val) {
  if (val < 256) {
    Serial.write(10);
    Serial.flush();
    delay(20);
  }
  if (val > 256 && val < 512) {
    Serial.write(20);
    Serial.flush();
    delay(20);
  }
  if (val > 512 && val < 768) {
    Serial.write(30);
    Serial.flush();
    delay(20);
  }
  if (val > 768) {
    Serial.write(40);
    Serial.flush();
    delay(20);
  }
}

