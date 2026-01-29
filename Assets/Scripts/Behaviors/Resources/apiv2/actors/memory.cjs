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

/**
 * Memory object for the current actor.
 * 
 * <p>Note: for most use cases, you should use card-level memory ({@link card}) instead.
 *
 * <p>This is an object behaviors can freely use to store their state. The
 * contents of this object will depend on what the actor's behaviors have
 * stored it in.
 *
 * <p>You can use this to store anything that the actor needs to remember
 * from one tick to the next, such as the state it is in, any counters,
 * etc.
 *
 * <p>Remember to reset any data to its default state on
 * {@link onResetGame} or {@link onInit}.
 *
 * <p>Note: an actor's memory is <i>private</i> to the actor, so other actors
 * can't access it. If you want to publish a value that other actors can
 * read, you can set it with {@link setVar}.
 *
 * @example
 * export function onTick() {
 *   if (!mem.asleep) {
 *     mem.sheepCount++;
 *     log("I counted " + mem.sheepCount + " sheep.");
 *     if (mem.sheepCount > 99) {
 *       // So sleepy...
 *       mem.asleep = true;
 *     }
 *   } else {
 *     log("Zzzzz....");
 *   }
 * }
 *
 * export function onResetGame() {
 *   mem.asleep = false;
 *   mem.sheepCount = 0;
 * }
 */
 
//处理消息时都会包在apiv2.js.txt的startHandlingMessage/endHandlingMessage间，这两个函数通过setUpGlobals_去设置全局变量（mem、card、props、temp）
var mem = {};

function getMem() {
  return mem;
}

function setMem(m) {
  mem = m;
}

/**
 * Memory object for the current behavior instance (card).
 *
 * <p>This is like memory, but is private to the current card.
 * So the data that you put into it does not
 * interfere with data inserted by cards.
 *
 * <p>Note: this is <i>private</i> to the card, so other cards
 * can't access it.
 *
 * @example
 * export function onTick() {
 *   if (!getCard().asleep) {
 *     getCard().sheepCount++;
 *     log("I counted " + getCard().sheepCount + " sheep.");
 *     if (getCard().sheepCount > 99) {
 *       // So sleepy...
 *       getCard().asleep = true;
 *     }
 *   } else {
 *     log("Zzzzz....");
 *   }
 * }
 *
 * export function onResetGame() {
 *   getCard().asleep = false;
 *   getCard().sheepCount = 0;
 * }
 */
var card = {};

function getCard() {
  return card;
}

function setCard(c) {
  card = c;
}

/**
 * Temporary memory for the current card/panel instance.
 *
 * <p>This is like {@link card}, but is temporary and only kept in
 * the local machine. It is not persisted to disk or synchronized
 * across the network, so it's only meant for short-term things
 * or for caching.</p>
 * 
 * <p>Unlike other kinds of memory, because this is not serialized,
 * you can freely store objects, functions, etc, anything you want.</p>
 * 
 * <p>Your script must be ready for this memory to be cleared
 * (that is, reset back to the empty object {}) at any time. This
 * will happen if the actor's network ownership is transferred,
 * and also on load.</p>
 * 
 * <p>This is also cleared on game reset, UNLIKE other memories!</p>
 * 
 * @example
 * export function onTick() {
 *   if (!getTemp().cachedExpensiveThing) {
 *     // Compute some expensive thing.
 *     getTemp().cachedExpensiveThing = computeSomeExpensiveThing();
 *   }
 *   // Use expensive thing here
 * }
 */
var temp = {};

function getTemp() {
  return temp;
}

function setTemp(t) {
  temp = t;
}

// HIDDEN (advanced). This declares that the current handler did not change memory.
var declareMemoryUnchanged = function () {
  ApiV2Context.instance.api.declareMemoryUnchanged();
}

// DEPRECATED
var saveRot = function (q) {
  assertQuaternion(q);
  return serializeQuaternion(q);
}

// DEPRECATED
var loadRot = function (saved) {
  return deserializeQuaternion(saved);
}

// DEPRECATED
var saveVec = function (v) {
  assertVector3(v);
  return { x: v.x, y: v.y, z: v.z };
}

// DEPRECATED
var loadVec = function (saved) {
  return vec3(saved.x, saved.y, saved.z);
}

// Auto-generated exports
exports.getCard = getCard;
exports.setCard = setCard;
exports.declareMemoryUnchanged = declareMemoryUnchanged;
exports.loadRot = loadRot;
exports.loadVec = loadVec;
exports.getMem = getMem;
exports.setMem = setMem;
exports.saveRot = saveRot;
exports.saveVec = saveVec;
exports.getTemp = getTemp;
exports.setTemp = setTemp;
