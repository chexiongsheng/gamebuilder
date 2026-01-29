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

export const PROPS = [
  propNumber("NeededScore", 3, {
    label: "Needed score"
  }),
  propEnum("WhoScores", "MYSELF", ["MYSELF", "ANOTHER_ACTOR"]),
  propActor("ScoringActor", "", {
    label: "Scoring actor",
    requires: [
      requireEqual("WhoScores", "ANOTHER_ACTOR")
    ]
  })
];

export function onPointScored(msg) {
  let targetPlayer = null;
  if (getProps().WhoScores === "ANOTHER_ACTOR") {
    if (exists(getProps().ScoringActor)) {
      targetPlayer = getControllingPlayer(getProps().ScoringActor);
    }
  } else {
    targetPlayer = getControllingPlayer(myself());
  }
  if (targetPlayer && msg.player === targetPlayer) {
    getCard().points += msg.amount || 1;
  }
}

export function onInit() {
  getCard().points = 0;
}

export function onCheck() {
  if (getCard().points >= getProps().NeededScore) {
    return {};
  }
}

export function getCardErrorMessage() {
  if (!isPlayerControllable() && getProps().WhoScores === "MYSELF") {
    return "Error: Actor is not a player.";
  }
}

export function getCardStatus() {
  const scorerScores = getProps().WhoScores === 'ANOTHER_ACTOR' ? `<color=yellow>${getDisplayName(getProps().ScoringActor)}</color> scores` : '<color=yellow>I</color> score';
  return {
    description: `When ${scorerScores} a total of <color=green>${getProps().NeededScore}</color> points`
  }
}