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
  propBoolean("SwitchBack", true, {
    label: 'Can switch back'
  }),
  propDeck('SwitchDeck', 'Event', {
    label: 'Switch when:',
    deckOptions: {
      defaultCardURIs: ['builtin:Collision Event Card']
    }
  }),
  propDeck('State1Deck', 'Action', {
    label: 'Initially do:',
  }),
  propDeck('State2Deck', 'Action', {
    label: 'After switch, do:',
  }),
];

const SIGNAL_COOLDOWN_TIME = 0.2;

export function onInit() {
  getCard().state = 0;
  getCard().event = {};
  getCard().lastEventTime = null;
}

export function onTick() {
  checkSwitch();
  callActionDeck(getCard().state === 0 ? "State1Deck" : "State2Deck", { event: getCard().event });
}

function checkSwitch() {
  if (getCard().state > 0 && !props.SwitchBack) return;
  const event = callEventDeck("SwitchDeck", true);
  if (event) {
    getCard().state = 1 - getCard().state;
  }
}

