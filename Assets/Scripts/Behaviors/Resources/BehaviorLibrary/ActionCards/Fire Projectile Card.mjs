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

import * as THREE from "three.mjs";
import { myself } from "../../apiv2/actors/actors.mjs";
import { getDisplayName } from "../../apiv2/actors/attributes.mjs";
import { clone } from "../../apiv2/actors/cloning.mjs";
import { getProps, propActor, propBoolean, propDecimal, propEnum, propSound, requireTrue } from "../../apiv2/actors/properties.mjs";
import { degToRad, vec3, vec3normalized, vec3scale, vec3zero, vec3z } from "../../apiv2/misc/math.mjs";
import { push } from "../../apiv2/physics/velocity.mjs";
import { getAimDirection } from "../../apiv2/player_controls/aiming.mjs";
import { isPlayerControllable } from "../../apiv2/player_controls/controls.mjs";
import { setVarPlease } from "../../apiv2/remote/remote.mjs";
import { playSound, Sounds } from "../../apiv2/sfx/sfx.mjs";
import { selfToWorldPos } from "../../apiv2/transform/position-get.mjs";
import { getForward } from "../../apiv2/transform/rotation-get.mjs";

const Quaternion = THREE.Quaternion;

export const PROPS = [
  propActor("Projectile", "builtin:LaserBolt", {
    pickerPrompt: "What should I shoot?",
    allowOffstageActors: true
  }),
  propSound("Sound", Sounds.LASER),
  propDecimal("Velocity", 30),

  propDecimal("OffsetX", 0),
  propDecimal("OffsetY", 1),
  propDecimal("OffsetZ", 2),

  propEnum("ShootDir", "CAMERA_AIM", [
    { value: "CAMERA_AIM", label: "Camera aim" },
    { value: "FORWARD", label: "Forward" }
  ]),

  propBoolean("HasRotationOffset", false, {
    label: "Offset rotation?"
  }),
  propDecimal("OffsetRotX", 0, {
    label: "X rotation offset",
    requires: [requireTrue("HasRotationOffset")]
  }),
  propDecimal("OffsetRotY", 0, {
    label: "Y rotation offset",
    requires: [requireTrue("HasRotationOffset")]
  }),
  propDecimal("OffsetRotZ", 0, {
    label: "Z rotation offset",
    requires: [requireTrue("HasRotationOffset")]
  }),
]

/**
 * @param {GActionMessage} actionMessage
 */
export function onAction(actionMessage) {
  // Calculate the position where we should spawn the projectile.
  const spawnPos = selfToWorldPos(vec3(getProps().OffsetX, getProps().OffsetY, getProps().OffsetZ));

  // Calculate the rotation of the projectile.
  const rot = computeShootRotation();

  // Compute shoot direction from rotation.
  const shootDir = vec3z();
  shootDir.applyQuaternion(rot);

  // Spawn the projectile.
  const proj = clone(getProps().Projectile, spawnPos, rot);

  // Set ourselves as the projectile's owner (for scoring).
  setVarPlease(proj, "owner", myself());

  // Push the projectile along our aim or forward direction.
  push(proj, vec3scale(shootDir, getProps().Velocity));

  // Play sound.
  if (getProps().Sound) {
    playSound(getProps().Sound);
  }
}

function computeShootRotation() {
  const baseShootDir = (isPlayerControllable() && getProps().ShootDir === "CAMERA_AIM") ?
    vec3normalized(getAimDirection()) : getForward();
  const mat = new THREE.Matrix4().lookAt(baseShootDir, vec3zero(), vec3(0, 1, 0));
  const rot = new Quaternion();
  rot.setFromRotationMatrix(mat);

  if (!getProps().HasRotationOffset) return rot;

  const euler = new THREE.Euler(
    degToRad(getProps().OffsetRotX), degToRad(getProps().OffsetRotY), degToRad(getProps().OffsetRotZ), 'YXZ');
  const quat = new Quaternion();
  quat.setFromEuler(euler);
  rot.multiply(quat);
  return rot;
}

export function onGetActionDescription() {
  return "Shoot";
}

export function getCardStatus() {
  return {
    description: `Fire projectile <color=white>${getDisplayName(getProps().Projectile)}</color> with velocity <color=yellow>${getProps().Velocity.toFixed(1)}</color>`
  }
}