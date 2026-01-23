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

// VISIBLE_TO_MONACO

function copyQuat(src, dst) {
  dst.x = src.x;
  dst.y = src.y;
  dst.z = src.z;
  dst.w = src.w;
}

// Three.js specific stuff. We'll probably just deprecate all the stuff above.

const ZERO3 = new THREE.Vector3(0, 0, 0);
const RIGHT_DIR = new THREE.Vector3(1, 0, 0);
const UP_DIR = new THREE.Vector3(0, 1, 0);
const FORWARD_DIR = new THREE.Vector3(0, 0, 1);
const ID_QUAT = new THREE.Quaternion(0, 0, 0, 1);

function lerp(a, b, t) {
  return a + t * (b - a);
}

function clamp(x, min, max) {
  if (min > max) {
    throw new Error('min must be less than or equal to max');
  }
  return Math.min(max, Math.max(min, x));
}

/** Returns the minimum of the two given numbers. */
function min(a, b) { return Math.min(a, b); }

/** Returns the maximum of the two given numbers. */
function max(a, b) { return Math.max(a, b); }

function damp(a, b, rate, dt) {
  return lerp(a, b, 1 - Math.exp(-rate * dt));
}

// Quat to angle-axis functions.
function radiansOf(threeQuat) {
  return 2 * Math.acos(threeQuat.w);
}

function axisOf(threeQuat, axisOut) {
  const qw = threeQuat.w;
  axisOut.set(
    threeQuat.x / Math.sqrt(1 - qw * qw),
    threeQuat.y / Math.sqrt(1 - qw * qw),
    threeQuat.z / Math.sqrt(1 - qw * qw));
  return axisOut;
}

function v2s(vec) {
  return `(${vec.x.toFixed(4)}, ${vec.y.toFixed(4)}, ${vec.z.toFixed(4)})`;
}

function randBetween(a, b) {
  return a + Math.random() * (b - a);
}

function for3d(mins, maxs, step, func) {
  const cursor = mins.clone();
  for (; cursor.x < maxs.x; cursor.x += step.x) {
    for (; cursor.y < maxs.y; cursor.y += step.y) {
      for (; cursor.z < maxs.z; cursor.z += step.z) {
        func(cursor);
      }
      cursor.z = mins.z;
    }
    cursor.y = mins.y;
  }
}

/* Here just for reference.
function generateTerrain() {
  const perlin = new ImprovedNoise();
  const L = 50;
  const H = 2;
  for3d(new THREE.Vector3(0, 0, 0), new THREE.Vector3(L, H, L), new THREE.Vector3(1, 1, 1),
    p => {
      const s = 5/L;
      const noise = perlin.noise(p.x*s, p.y*s, p.z*s);
      sysLog(JSON.stringify(p));
      createStaticSceneThing('Brick',
          p, ID_QUAT.clone(),
          {r:noise, g:0, b:0, a:1});
    });
}
*/

runUnitTests('math', {
  testClamp: function () {
    assert(clamp(2, 1, 3) == 2);
    assert(clamp(-2, -3, -1) == -2);
    assert(clamp(4, 1, 3) == 3);
    assert(clamp(-4, 1, 3) == 1);
    assert(clamp(-4, -3, -1) == -3);
    assert(clamp(2, -3, -1) == -1);
  },
});

// ESM exports
export { FORWARD_DIR };
export { ID_QUAT };
export { RIGHT_DIR };
export { UP_DIR };
export { ZERO3 };
export { axisOf };
export { clamp };
export { copyQuat };
export { damp };
export { for3d };
export { generateTerrain };
export { lerp };
export { max };
export { min };
export { radiansOf };
export { randBetween };
export { v2s };
