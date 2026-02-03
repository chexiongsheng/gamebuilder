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

public static class CodeTemplates
{
  public const string ACTION =
@"// Example Action card.
// The onAction function will be called when this card
// is activated.
import { getProps, propNumber } from ""../../apiv2/actors/properties.mjs"";
import { moveUp } from ""../../apiv2/transform/position-set.mjs"";

export const PROPS = [
  propNumber('distance', 1)
];

export function onAction() {
  // Example action: move the actor up a bit.
  moveUp(getProps().distance);

  // To print a log message, uncomment this: (press ~ to show console)
  //log('Action triggered!')
}
";

  public const string EVENT =
@"// Example Event card.
// An event card is supposed to detect that something
// interesting happened. It is normally used in
// conjuction with action cards in an IF-THEN panel.
// Here is an example that triggers on a collision:

// When we collide, store the event in card.triggeredEvent.
import { getCard } from ""../../apiv2/actors/memory.mjs"";

export function onCollision(msg) {
  getCard().triggeredEvent = {
    actor: msg.other
  };
}

// This is the checking function that gets called every frame
// to check if the event happened.
export function onCheck() {
  if (getCard().triggeredEvent) {
    // We have an event to deliver, so deliver it now.
    const rv = getCard().triggeredEvent;
    delete getCard().triggeredEvent;

    // To print a log message, uncomment this: (press ~ to show console)
    //log('Event triggered!')

    return rv;
  }
  // Otherwise, nothing to report.
}
";

  public const string MISC =
@"// Example card.

// User-editable properties for this card:
import { getProps, propNumber } from ""../../apiv2/actors/properties.mjs"";
import { cooldown, send } from ""../../apiv2/actors/messages.mjs"";
import { moveForward, moveRight } from ""../../apiv2/transform/position-set.mjs"";

export const PROPS = [
  propNumber('speed', 2)
];

// onTick is called every frame (50-60 times per second).
export function onTick() { 
  // Uncomment the line below to make the actor move forward, at the
  // speed that was set in the properties.
  // moveForward(getProps().speed);
}

// onCollision is called when the actor collides with another actor:
export function onCollision(msg) {
  // To print a log message, uncomment this: (press ~ to show console)
  //log('I collided with ' + getDisplayName(msg.other));

  // onCollision() get called continually while the actors are colliding,
  // so if you want to put a delay between calls, use a cooldown:
  cooldown(1);
}
";
}
