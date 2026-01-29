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

import { exists, myself } from "../../apiv2/actors/actors.mjs";
import { clone } from "../../apiv2/actors/cloning.mjs";
import { getProps, propActor, propNumber } from "../../apiv2/actors/properties.mjs";
import { vec3 } from "../../apiv2/misc/math.mjs";
import { getPos } from "../../apiv2/transform/position-get.mjs";
import { getForward, getRight, getRot, getUp } from "../../apiv2/transform/rotation-get.mjs";
import { logError } from "../../util.mjs";
import { kick } from "../../voosMain.mjs";

// Fire Projectile<size=70%>\nShoot clones of another object, like a baseball.

export const PROPS = [
  propActor("Projectile", true),
  propNumber("Velocity", 15),
  propNumber("ProjectileOffsetX", 0),
  propNumber("ProjectileOffsetY", 0),
  propNumber("ProjectileOffsetZ", 2)
];

// tag ability

//fire the projectile
export function onAction() {

  //if no projectile, cancel and send a log message
  if (!exists(getProps().Projectile)) {
    logError("Projectile has no target set! Fix this in the Inspector!");
    return;
  }

  //if projectile is self, cancel and send a log message
  if (myself() == getProps().Projectile) {
    logError("Actor can't be it's own projectile! Fix this in the Inspector!");
    return;
  }

  //find shooting direction
  const dir = findProjectileDirection();

  //determine spawn offset of projectile (so I dont just shoot myself)
  const pos = getPos();

  const posOffset = vec3(getProps().ProjectileOffsetX,
    getProps().ProjectileOffsetY,
    getProps().ProjectileOffsetZ);
  pos.add(getLocalVec3(posOffset));

  //set projectil rotation to match mine
  const rot = getRot();

  //create an instance of the projectile
  const projectile = clone(getProps().Projectile, pos, rot);

  //apply velocity to the projectile based on shooting direction
  kick(projectile, dir.multiplyScalar(getProps().Velocity));
}

// replace this with aim direction
function findProjectileDirection() {
  return getForward();
}

// please move this functionality into API
function getLocalVec3(localVec) {
  let newVec = getRight(localVec.x);
  newVec.add(getUp(localVec.y));
  newVec.add(getForward(localVec.z));
  return newVec;
}