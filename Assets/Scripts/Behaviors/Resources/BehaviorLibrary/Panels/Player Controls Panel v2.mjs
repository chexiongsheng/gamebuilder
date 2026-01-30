/*
 * Copyright 2019 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     https://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */


import { exists, getActors, myself } from "../../apiv2/actors/actors.mjs";
import { deleteVar, getVar, setVar } from "../../apiv2/actors/attributes.mjs";
import { getCard } from "../../apiv2/actors/memory.mjs";
import { send, sendToSelf } from "../../apiv2/actors/messages.mjs";
import { propBoolean, propEnum, requireFalse } from "../../apiv2/actors/properties.mjs";
import { deltaTime, getTime } from "../../apiv2/misc/time.mjs";
import { getAllPlayers, getPlayerByNumber, getPlayerNumber, playerExists } from "../../apiv2/multiplayer/players.mjs";
import { getControllingPlayer, setCameraActor, setControllingPlayer, setIsPlayerControllable } from "../../apiv2/player_controls/controls.mjs";
import { setTintHex } from "../../apiv2/rendering/color.mjs";
import { UiColor, uiGetScreenHeight, uiGetScreenWidth, uiGetTextWidth, uiRect, uiText } from "../../apiv2/ui/widgets.mjs";
import { assert } from "../../testing.mjs";;

export const PROPS = [
  // Note: this is an enum just for UX purposes; the underlying value is always
  // parsed as an integer. If you create values that are not integers here,
  // also update PlayerControllableWizard appropriately.
  propEnum('PlayerNumber', '1', [
    { value: '0', label: 'NOBODY' },
    { value: '1', label: 'Player 1' },
    { value: '2', label: 'Player 2' },
    { value: '3', label: 'Player 3' },
    { value: '4', label: 'Player 4' },
    { value: '5', label: 'Player 5' },
    { value: '6', label: 'Player 6' },
    { value: '7', label: 'Player 7' },
    { value: '8', label: 'Player 8' },
    { value: '9', label: 'Player 9' },
    { value: '10', label: 'Player 10' },
    { value: '11', label: 'Player 11' },
    { value: '12', label: 'Player 12' },
    { value: '13', label: 'Player 13' },
    { value: '14', label: 'Player 14' },
    { value: '15', label: 'Player 15' },
    { value: '16', label: 'Player 16' },
  ], {
    label: "Controlled by",
    requires: [requireFalse('AutoAssign')]
  }),
  propBoolean('AutoAssign', false, {
    label: 'Auto assign'
  }),

  propBoolean('AutoColor', false, {
    label: "Automatic color"
  })
  // *** DO NOT ADD propDeck PROPERTIES TO THIS PANEL.
  // See PLAYER_CONTROLS_PANEL_HAS_NO_DECKS_ASSUMPTION in the source code!
];

const PLAYER_COLORS = [
  "#ffffff",
  "#a00000",
  "#00a000",
  "#a0a000",
  "#0000a0",
  "#a000a0",
  "#00a0a0",
  "#a0a0a0",
]

export function onInit() {
  // If present, assignedPlayer is the player that we are assigned to until further notice.
  // This may be null to mean we are (intentionally) assigned to nobody, so this being null
  // is different from it being undefined.
  delete getCard().assignedPlayer;
  getCard().lastAutoAssignCheck = getTime();
  getCard().lastPlayerNumber = null;
  getCard().oldSelf = myself(); // to detect copy/paste
  delete getCard().activeControlLocks;
}

function getDesiredPlayerNumber() {
  // For backward compat (we changed the prop type from number to enum).
  return +(getProps().PlayerNumber || 0);
}

export function onTick() {
  setVar("hasPlayerControlsPanel", true);
  setIsPlayerControllable(true);
  // If we were just copy/pasted, reinit.
  if (myself() !== getCard().oldSelf) {
    onInit();
  }
  // If we were assigned to a player and the player no longer exists, unassign.
  if (getCard().assignedPlayer !== undefined && getCard().assignedPlayer !== null && !playerExists(getCard().assignedPlayer)) {
    delete getCard().assignedPlayer;
  }

  if (getCard().assignedPlayer !== undefined) {
    // If there is a getCard().assignedPlayer, that takes precedence over everything.
    setVar("playerAutoAssignAvailable", false);
    setControllingPlayer(getCard().assignedPlayer); // might be null to mean "nobody"
  } else if (getProps().AutoAssign) {
    setVar("playerAutoAssignAvailable", true);
    maybeAutoAssign();
  } else {
    setVar("playerAutoAssignAvailable", false);
    // Otherwise, go by player#.
    const desiredPlayerNumber = getDesiredPlayerNumber();
    const playerId = desiredPlayerNumber > 0 ? getPlayerByNumber(desiredPlayerNumber) : null;
    setControllingPlayer(playerId);
  }
  if (getProps().AutoColor) {
    updateColor();
  }
  drawControlsLock();
  dialogueOnTick();
}

export function onCardRemoved() {
  deleteVar("hasPlayerControlsPanel", false);
  setControllingPlayer(null);
  setCameraActor(null);
  setIsPlayerControllable(false);
}

function maybeAutoAssign() {
  // Run this logic once every 2 seconds, not every frame.
  if (getTime() < (getCard().lastAutoAssignCheck || 0) + 2) {
    return;
  }
  const allActors = getActors();
  getCard().lastAutoAssignCheck = getTime();
  // If we don't have priority, yield.
  if (!doWeHaveAutoAssignPriority(allActors)) return;
  // Okay, we have priority, so check if there is a player without an actor.
  const playerHasActor = {};
  for (const actor of allActors) {
    const player = getControllingPlayer(actor);
    if (player) playerHasActor[player] = true;
  }
  for (const playerId of getAllPlayers()) {
    if (!playerHasActor[playerId]) {
      getCard().assignedPlayer = playerId;
      return;
    }
  }
}

function updateColor() {
  const player = getControllingPlayer();
  const playerNumber = player ? (getPlayerNumber(player) || 0) : 0;
  if (playerNumber !== getCard().lastPlayerNumber) {
    setTintHex(PLAYER_COLORS[playerNumber % PLAYER_COLORS.length]);
    getCard().lastPlayerNumber = playerNumber;
  }
}

function doWeHaveAutoAssignPriority(allActors) {
  // The actor that has auto assign priority is the one with the lowest ID.
  for (const actor of allActors) {
    if (exists(actor) && getVar("playerAutoAssignAvailable", actor) && actor < myself()) {
      return false;
    }
  }
  return true;
}

// AssignPlayer message requests us to assign to a given player until further notice.
// msg.playerId is the player ID to assign to, or null to unassign.
export function onAssignPlayer(msg) {
  assert(msg.playerId === null || typeof msg.playerId === 'string', 'msg.playerId in AssignPlayer message must be string or null');
  getCard().assignedPlayer = msg.playerId;
}

// UnassignPlayer message requests us to unassign, returning to our default behavior.
export function onUnassignPlayer() {
  delete getCard().assignedPlayer;
}

// DEPRECATED:
export function onRequestSetCamera(msg) {
  if (exists(msg.cameraActor)) {
    setCameraActor(msg.cameraActor);
  }
}

// Someone is requesting us to lock player controls.
export function onLockControls(msg) {
  assert(msg.name, "LockControls: msg.name must be non-empty");
  getCard().activeControlLocks = getCard().activeControlLocks || {};
  getCard().activeControlLocks[msg.name] = msg.debugString || "";
  setVar("ControlsLocked", true);
}

// Someone is requesting us to unlock player controls.
export function onUnlockControls(msg) {
  assert(msg.name, "UnlockControls: msg.name must be non-empty");
  if (getCard().activeControlLocks) delete getCard().activeControlLocks[msg.name];
  setVar("ControlsLocked", getCard().activeControlLocks && Object.keys(getCard().activeControlLocks).length > 0);
}

function drawControlsLock() {
  if (!getVar("ControlsLocked")) return;
  uiText(800, uiGetScreenHeight() - 30, "[Controls locked]", UiColor.WHITE, { opacity: 0.8, center: true });
}

export function onResetGame() {
  dialogueResetGame();
}

export function onKeyDown(msg) {
  dialogueOnKeyDown(msg);
}


/* ================================================================================================ */
/* DIALOGUE FUNCTIONALITY */
/* ================================================================================================ */

const DIA_LOCK_NAME = "DialoguePanel";
const DIA_PADDING = 30;
const DIA_LINE_HEIGHT = 25;
const DIA_SPEAKER_LINE_HEIGHT = 40;
const DIA_SPACE_BETWEEN_TEXT_AND_REPLIES = 30;
const DIA_KEY_CHOICES = {
  "return": -1,
  "enter": -1,
  "1": 0,
  "2": 1,
  "3": 2,
  "[1]": 0,
  "[2]": 1,
  "[3]": 2,
}

function dialogueResetGame() {
  delete getCard().dia;
}

// msg.requester: the actor requesting the dialogue
// msg.speaker: the name of the speaker
// msg.color: the color to use when displaying the speaker name
// msg.text: the text to speak
// msg.cps: text speed in characters per second
// msg.replies[]: possible replies, each:
//     text: the text of the reply
//     message: message to send to the requester when this reply is chosen
export function onLaunchDialogue(msg) {
  if (getCard().dia) return;
  getCard().dia = JSON.parse(JSON.stringify(msg));
  getCard().dia.startTime = getTime();
  getCard().dia.text = getCard().dia.text || "Missing dialogue text";
  getCard().dia.replies = getCard().dia.replies || [];
  const textLines = msg.text.split("\n").length;

  let textWidth = uiGetTextWidth(msg.text);
  for (const reply of getCard().dia.replies) {
    textWidth = Math.max(textWidth, uiGetTextWidth("[1]: " + reply.text));
  }

  const width = textWidth + 2 * DIA_PADDING;
  let height = textLines * DIA_LINE_HEIGHT +
    (getCard().dia.speaker ? DIA_SPEAKER_LINE_HEIGHT : 0) +
    2 * DIA_PADDING +
    DIA_SPACE_BETWEEN_TEXT_AND_REPLIES +
    DIA_LINE_HEIGHT * (getCard().dia.replies || [0]).length;
  getCard().dia.rect = {
    x: (uiGetScreenWidth() - width) / 2,
    y: (uiGetScreenHeight() - height) / 2,
    w: width,
    h: height
  };
  getCard().dia.animating = true;
  getCard().dia.charsShown = 0;
  sendToSelf("LockControls", { name: DIA_LOCK_NAME });
}

// onTick, not onLocalTick because we only want to show UI on THIS player.
function dialogueOnTick() {
  if (!getCard().dia) return;
  if (getCard().dia.animating) {
    getCard().dia.charsShown += (getCard().dia.cps || 20) * deltaTime();
    if (getCard().dia.charsShown >= getCard().dia.text.length) {
      getCard().dia.animating = false;
    }
  }
  const textToPrint = getCard().dia.text.substr(0, Math.ceil(getCard().dia.charsShown));
  uiRect(getCard().dia.rect.x, getCard().dia.rect.y, getCard().dia.rect.w, getCard().dia.rect.h, 0x000020, { opacity: 0.85 });
  uiRect(getCard().dia.rect.x, getCard().dia.rect.y, getCard().dia.rect.w, getCard().dia.rect.h, 0xffffff, { style: "BORDER" });

  let y = getCard().dia.rect.y + DIA_PADDING;

  if (getCard().dia.speaker) {
    uiText(getCard().dia.rect.x + DIA_PADDING, y, "[ " + getCard().dia.speaker + " ]", getCard().dia.color);
    y += DIA_SPEAKER_LINE_HEIGHT;
  }

  for (const line of textToPrint.split("\n")) {
    uiText(getCard().dia.rect.x + DIA_PADDING, y, line);
    y += DIA_LINE_HEIGHT;
  }
  y += DIA_SPACE_BETWEEN_TEXT_AND_REPLIES;

  if (getCard().dia.animating) return;

  const repliesX = getCard().dia.rect.x + DIA_PADDING;

  // If no replies, just show prompt to press ENTER.
  if (getCard().dia.replies.length === 0) {
    blinkText(repliesX, y, "[ ENTER ]");
    return;
  }

  // Show possible replies.
  for (let i = 0; i < getCard().dia.replies.length; i++) {
    blinkText(repliesX, y, "[" + (i + 1) + "]");
    uiText(repliesX + 60, y, getCard().dia.replies[i].text);
    y += DIA_LINE_HEIGHT;
  }
}

function dialogueOnKeyDown(msg) {
  if (!getCard().dia) return;

  const choice = DIA_KEY_CHOICES[msg.keyName];
  if (getCard().dia.replies.length === 0 && choice === -1) {
    dismissDialogue();
  }
  if (choice >= 0 && choice < getCard().dia.replies.length) {
    // Reply chosen.
    if (exists(getCard().dia.requester)) {
      send(getCard().dia.requester, getCard().dia.replies[choice].message);
    }
    dismissDialogue();
  }
}

function blinkText(x, y, text) {
  const color = getTime() % 1 < 0.5 ? 0x000000 : 0x00ff00;
  uiText(x, y, text, color);
}

function dismissDialogue() {
  sendToSelf("UnlockControls", { name: DIA_LOCK_NAME });
  delete getCard().dia;
}