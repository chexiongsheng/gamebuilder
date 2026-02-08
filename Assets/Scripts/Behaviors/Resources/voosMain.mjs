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
import { assert, runUnitTests } from "./testing.mjs";
import { assertBoolean, assertNumber, assertQuaternion, assertString, assertStringOrNull, assertVector3, flattenArray, mapGetOrCreate, parseJsonOrEmpty, serializeQuaternion } from "./util.mjs";;
import { ModuleBehaviorSystem, getBehaviorProperties } from "./ModuleBehaviorSystem.mjs";
import { VoosBinaryReaderWriter } from "./serialization.mjs";
import { Queue } from "./Queue.src.mjs";
import { ApiV2Context } from "./apiv2/apiv2.mjs";
import { packObj } from "./pack-unpack.mjs";
import { Actor } from "./ModuleBehaviorsActor.mjs";
import { setProfileEnable, beginProfileSample, endProfileSample } from "./util.mjs";
import { send } from "./apiv2/actors/messages.mjs";
import { Quaternion, Vector3 } from "./threejs-overrides.mjs";
import { push } from "./apiv2/physics/velocity.mjs";
import { log } from "./apiv2/misc/utility.mjs";
import { setMemCheckMode } from "./ModuleBehaviorsActor.mjs";

// ==================== Actor Property Accessors ====================

// Boolean accessors
function getActorBoolean(actorId, fieldId) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  return globalThis.__voosEngine.GetActorBoolean(Number(actorId), Number(fieldId));
};

function setActorBoolean(actorId, fieldId, value) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  globalThis.__voosEngine.SetActorBoolean(Number(actorId), Number(fieldId), Boolean(value));
};

// Float accessors
function getActorFloat(actorId, fieldId) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  return globalThis.__voosEngine.GetActorFloat(Number(actorId), Number(fieldId));
};

function setActorFloat(actorId, fieldId, value) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  globalThis.__voosEngine.SetActorFloat(Number(actorId), Number(fieldId), Number(value));
};

// Vector3 accessors
function getActorVector3(actorId, fieldId, pos) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  // C# out parameters need to be wrapped with $ref and unwrapped with $unref
  const outX = puer.$ref();
  const outY = puer.$ref();
  const outZ = puer.$ref();
  globalThis.__voosEngine.GetActorVector3(Number(actorId), Number(fieldId), outX, outY, outZ);
  pos.x = puer.$unref(outX);
  pos.y = puer.$unref(outY);
  pos.z = puer.$unref(outZ);
};

function setActorVector3(actorId, fieldId, x, y, z) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }

  // Pass x, y, z as separate parameters
  globalThis.__voosEngine.SetActorVector3(
    Number(actorId),
    Number(fieldId),
    x,
    y,
    z
  );
};

// Quaternion accessors
function getActorQuaternion(actorId, fieldId, quaternion) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  // C# out parameters need to be wrapped with $ref and unwrapped with $unref
  const outX = puer.$ref();
  const outY = puer.$ref();
  const outZ = puer.$ref();
  const outW = puer.$ref();
  globalThis.__voosEngine.GetActorQuaternion(Number(actorId), Number(fieldId), outX, outY, outZ, outW);

  quaternion.x = puer.$unref(outX);
  quaternion.y = puer.$unref(outY);
  quaternion.z = puer.$unref(outZ);
  quaternion.w = puer.$unref(outW);

};

function setActorQuaternion(actorId, fieldId, x, y, z, w) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }

  // Pass x, y, z, w as separate parameters
  globalThis.__voosEngine.SetActorQuaternion(
    Number(actorId),
    Number(fieldId),
    x,
    y,
    z,
    w
  );
};

// String accessors
function getActorString(actorId, fieldId) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  return globalThis.__voosEngine.GetActorString(Number(actorId), Number(fieldId));
};

function setActorString(actorId, fieldId, value) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  globalThis.__voosEngine.SetActorString(Number(actorId), Number(fieldId), String(value));
};

// ==================== Service Call API ====================

// callVoosService - Main service call API (compatible with V8 engine)
// Note: This is a synchronous-looking API but internally uses async callback
// It's kept for backward compatibility with existing code
function callVoosService(serviceName, arg) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }

  // For synchronous-looking API, we use a workaround:
  // Store result in a closure and return it synchronously
  // This works because Unity's main thread will process the callback immediately
  let result = undefined;
  let error = null;
  let completed = false;

  try {
    const argsJson = JSON.stringify(arg);

    globalThis.__voosEngine.CallServiceForPuerts(serviceName, argsJson, function (resultJson) {
      completed = true;
      if (resultJson && resultJson !== '') {
        try {
          result = JSON.parse(resultJson);
        } catch (e) {
          result = resultJson;
        }
      }
    });

    // In Unity's single-threaded environment, the callback should execute immediately
    if (!completed) {
      console.error('callVoosService: callback not executed immediately');
    }

    return result;
  } catch (err) {
    console.error('callVoosService error:', err.message);
    throw err;
  }
};

// sysLog - System logging API (alias for log)
function sysLog(...args) {
  const message = args.map(arg => {
    if (typeof arg === 'object') {
      try {
        return JSON.stringify(arg);
      } catch (e) {
        return String(arg);
      }
    }
    return String(arg);
  }).join(' ');

  if (globalThis.__voosEngine) {
    globalThis.__voosEngine.HandleLogForPuerts('log', message);
  }
};

// ==================== Global Error Handler ====================

globalThis.addEventListener = globalThis.addEventListener || function () { };
globalThis.removeEventListener = globalThis.removeEventListener || function () { };

// Capture unhandled errors
const originalErrorHandler = globalThis.onerror;
globalThis.onerror = function (message, source, lineno, colno, error) {
  if (globalThis.__voosEngine) {
    const errorMessage = error ? error.message : String(message);
    const stackTrace = error ? error.stack : `at ${source}:${lineno}:${colno}`;
    globalThis.__voosEngine.HandleErrorForPuerts(errorMessage, stackTrace);
  }

  if (originalErrorHandler) {
    return originalErrorHandler.apply(this, arguments);
  }
  return false;
};

let moduleBehaviors_ = new ModuleBehaviorSystem();

// State within a single update call
let response = null;
let currDeltaSeconds = 0;
let updateCount = 0;

assert(typeof getActorString == 'function');
assert(typeof setActorString == 'function');

function getActorColor(actorId, fieldId, colorOut) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  const outR = puer.$ref();
  const outG = puer.$ref();
  const outB = puer.$ref();
  const outA = puer.$ref();
  globalThis.__voosEngine.GetActorColor(Number(actorId), Number(fieldId), outR, outG, outB, outA);
  colorOut.r = puer.$unref(outR);
  colorOut.g = puer.$unref(outG);
  colorOut.b = puer.$unref(outB);
  colorOut.a = puer.$unref(outA);
}

function setActorColor(actorId, fieldId, newValue) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  globalThis.__voosEngine.SetActorColor(Number(actorId), Number(fieldId), Number(newValue.r), Number(newValue.g), Number(newValue.b), Number(newValue.a));
}

// TODO take a sender actor here as well.
function enqueueRemoteMessage(targetActor, messageType, messageArgs) {
  if (messageArgs === undefined) {
    messageArgs = {};
  }

  // TODO let's just make this another callVoosService.
  // See VoosEngine.ActorMessage
  response.messagesToRemoteActors.push({
    name: messageType,
    targetActor: targetActor,
    argsJson: JSON.stringify(packObj(messageArgs))
  });
}

function queueMessageToUnity(targetActor, message) {
  response.messagesToUnity.push({ targetActor: targetActor, name: message });
}

let initBehaviorsPending = true;

let cachedPlayerActors = null;

function getPlayerActorsCached() {
  if (cachedPlayerActors === null) {
    cachedPlayerActors = callVoosService("GetPlayerActors");
  }
  return cachedPlayerActors;
}

function tickWorld(request, binaryBytes) {
  try {
    cachedPlayerActors = null;
    setProfileEnable(request.enableProfilingService);
    setMemCheckMode(request.memCheckMode);
    ApiV2Context.setup(request.deltaSeconds);
    updateCount++;
    const reader = new VoosBinaryReaderWriter(binaryBytes);

    moduleBehaviors_.prepareForTick();

    beginProfileSample("deserializeOrderedActors");
    // 同步Actor，比如新建的new，删除的delete，new的时候只记录个名字
    moduleBehaviors_.deserializeOrderedActors(reader);
    endProfileSample();

    beginProfileSample("deserializeActorStateSync");
    // 逐个调用Actor.deserialize反序列化状态，其中比较大的是转成json的memory（可选）
    // 数据来自C#的VoosActor.SerializeForScriptSync
    // 看代码本地actor是不会有从C#传过来的memory，但网络下会来自RPC
    moduleBehaviors_.deserializeActorStateSync(reader);
    endProfileSample();

    beginProfileSample("merge player actors");
    // 来自PushPlayerActorStateToSerialized，只有GetIsPlayerControllable为true的actor才会被记录
    moduleBehaviors_.mergeActorJsonObjects(request.runtimeState.actors);
    endProfileSample();

    // Some hacky global vars...unfortunately many functions still refer to these directly.
    currDeltaSeconds = request.deltaSeconds;

    // Unfortunately, right now the V8 native embedding uses the same object
    // for input and output.
    response = request;
    Object.assign(response, {
      playerToolTips: [],
      velocityChanges: [],
      torqueRequests: [],
      messagesToRemoteActors: [],
      messagesToUnity: []
    });

    if (initBehaviorsPending) {
      initBehaviorsPending = false;
      beginProfileSample("init new behavior uses");
      moduleBehaviors_.initNewBehaviorUses(request.runtimeState.gameTime);
      endProfileSample();
    }

    const namesById = moduleBehaviors_.getNamesById();

    // JSON-free fast path for collisions. It's important we do this before any
    // other messages to preserve order. These are enqueued during FixedUpdate,
    // which happens before VoosEngine.Update or any RPCs are pumped, etc.
    // Otherwise, this leads to issues like the "reset-touching-checkpoint"
    // problem.
    beginProfileSample("queueCollisions");
    const numCollisions = reader.readInt32();
    for (let i = 0; i < numCollisions; i++) {
      const receiverId = reader.readUint16();
      const otherId = reader.readUint16();
      const isEnter = reader.readBoolean();
      const receiver = namesById[receiverId];
      const other = namesById[otherId];
      if (!receiver || !other) continue;
      // 通过序列化传过来的碰撞消息通过id来标识接收的actor
      // 信息来自C#的VoosEngine.PumpQueuedCollisions
      // sendMessage只是入队列，在update才调用pumpMessageQueue_统一分发消息
      // pumpMessageQueue_会调用到Actor的handleMessage
      // handleMessage调用handleMessageImmediate_
      // handleMessageImmediate_会根据brainName获取brain
      // 然后根据每个use去调用handleMessageForUse
      // handleMessageForUse会从contextMapsByUse_[use][message] 获取MessageHandlingContext，MessageHandlingContext会持有回调函数
      // 然后调用MessageHandlingContext的handleMessage
      // 然后MessageHandlingContext的handleMessage会切换上下文（setUpGlobals_）然后调用invokeHandler_
      // invokeHandler_中拆出消息的data作为参数1，消息名包成另外一个消息作为参数2调用
      moduleBehaviors_.sendMessage(receiver, "Collision", { other: other });
      if (isEnter) {
        // Legacy
        moduleBehaviors_.sendMessage(receiver, "TouchEnter", { other: other });
      }
    }
    const sanityCheck = reader.readInt32();
    assert(sanityCheck == 536);
    endProfileSample();

    beginProfileSample("queueTerrainCollisions");
    const numTerrainCollisions = reader.readInt32();
    for (let i = 0; i < numTerrainCollisions; i++) {
      const receiverId = reader.readUint16();
      const blockStyle = reader.readUint16();
      const receiver = namesById[receiverId];
      if (!receiver) continue;
      moduleBehaviors_.sendMessage(receiver, "TerrainCollision", { blockStyle: blockStyle });
    }
    const terrainSanityCheck = reader.readInt32();
    assert(terrainSanityCheck == 451);
    endProfileSample();

    beginProfileSample("queue tick");
    moduleBehaviors_.sendMessageToAll('Tick',
      { dt: request.deltaSeconds, time: request.runtimeState.gameTime }, null,
      { onstage: true, offstage: false });
    moduleBehaviors_.sendMessageToAll('OffstageTick',
      { dt: request.deltaSeconds, time: request.runtimeState.gameTime }, null,
      { onstage: false, offstage: true });
    // LocalTick gets delivered locally even to actors that are remote so that people can
    // do multiplayer UI, etc.
    moduleBehaviors_.sendMessageToAll('LocalTick',
      { dt: request.deltaSeconds, time: request.runtimeState.gameTime }, null,
      { onstage: true, offstage: false });
    endProfileSample();

    beginProfileSample("queue msgs from unity");
    request.messagesFromUnity.forEach(message => {
      const messageArgs = parseJsonOrEmpty(message.argsJson);
      if (message.targetActor != null && message.targetActor != "") {
        if (moduleBehaviors_.doesActorExist(message.targetActor)) {
          moduleBehaviors_.sendMessage(message.targetActor, message.name, messageArgs);
        }
        else {
          // It's very possible that the actor was destroyed after the Unity
          // collision event but before VoosUpdate, such as by an RPC. This
          // repro's well with funktronic/FINAL_BUILD_4. So just ignore it,
          // rather than trying to send to a non-existant actor.
        }
      } else {
        // NOTE: all broadcasts from Unity are sent to all actors (onstage and offstage).
        // If in the future we have cases where this shouldn't happen, we should add a parameter
        // to control this.
        moduleBehaviors_.sendMessageToAll(message.name, messageArgs, null,
          { onstage: true, offstage: true, fromRemote: message.fromRemote });
      }
    });
    endProfileSample();

    // Now that a bunch of messages are queued up, let the system update and effectively pump the queue.
    beginProfileSample("full update call");
    moduleBehaviors_.update(request.deltaSeconds, request.runtimeState.gameTime);
    endProfileSample();

    // Do any end-of-frame operations required by APIv2.
    ApiV2Context.instance.endFrame();

    // Note that, this is actually not the end of the frame. The true end is the
    // 'postMessageFlush'. Maybe we could've exposed that as a binding, so we
    // could flush it from javascript..right now we have an additional callback
    // that is called after the flush, but I think having JS drive the flush
    // would be cleaner!

  } catch (e) {
    sysLog(e.stack);
    throw e;
  } finally {
    ApiV2Context.tearDown();
  }
}

function updateBehaviorDatabase(request) {
  const brainsHandlingCollisions = moduleBehaviors_.resetDatabase(request.jsonObject);
  delete request.jsonObject;
  request.brainsHandlingCollisions = brainsHandlingCollisions;
  initBehaviorsPending = true;
}

function updateAgent(request, arrayBuffer) {
  beginProfileSample("updateAgent");
  if (request.operation == 'updateBehaviorDatabase') {
    //console.error(`updateBehaviorDatabase ${JSON.stringify(request, null, 4)}`);
    //console.error(`updateBehaviorDatabase`);
    // 游戏启动一次
    // TODO: 我选中一个actor点击Logic就有N次（还没改），对于每个panel一次，这应该可以优化
    // 每次都修改都会同步全量，可能是为了实现undo/redo简单些，修改前新建一个UndoScope，构造函数会克隆当前整个数据库作为undo的依据，然后执行修改，然后又克隆一次作为redo的依据
    updateBehaviorDatabase(request);
  }
  else if (request.operation == 'getBehaviorProperties') {
    const props = getBehaviorProperties(request.behaviorUri);
    request.propsJson = JSON.stringify({ props: props });
  }
  else if (request.operation == 'tickWorld') {
    //console.error(`updateBehaviorDatabase ${JSON.stringify(request, null, 4)}`);
    tickWorld(request, arrayBuffer);
  }
  else if (request.operation == 'callBehaviorUseMethod') {
    const returnValue = moduleBehaviors_.callBehaviorUseMethod(request.useId, request.actorId, request.methodName, request.args);
    request.returnValue = returnValue;
  }
  else if (request.operation == 'getActorMemoryJson') {
    const actor = moduleBehaviors_.getActor(request.actorId);
    if (actor === undefined) {
      throw new Error(`Could not find actor by ID ${request.actorId}`);
    }
    request.memoryJson = actor.getMemoryJson();
  }

  // bit of a hack..always restore this.
  setProfileEnable(true);
  endProfileSample();
}

function postMessageFlush(response, actorsBytes) {
  if (response.operation == 'tickWorld') {
    beginProfileSample("serializeDirtyMemoryActors");
    moduleBehaviors_.serializeDirtyMemoryActors(actorsBytes);
    endProfileSample();
    // Reduce potential size of response..
    delete response.runtimeState;
  }
}

// Note: this is a deprecated APIv1 function. For newer code in APIv2,
// use push(). Which is the same, but has a better name that doesn't
// imply feet.
function kick(targetName, velocityChange) {
  assertString(targetName, 'targetName');
  assertVector3(velocityChange, 'velocityChange');
  moduleBehaviors_.sendMessage(targetName, 'AddVelocityChange', { velocityChange: velocityChange });
}

const TIMERS_MEMORY_KEY = '__timers__';

// Updates the timer. Returns true if time is up.
// NOTE: Also returns **true** if the timer was never set!
// TODO put this in handler API
function everySeconds(seconds, key, memory) {
  if (memory[TIMERS_MEMORY_KEY] == undefined) {
    memory[TIMERS_MEMORY_KEY] = {};
  }
  const timers = memory[TIMERS_MEMORY_KEY];

  if (timers[key] == undefined) {
    // set it and immediately return true.
    timers[key] = seconds;
    return true;
  }

  timers[key] -= currDeltaSeconds;

  if (timers[key] < 0.0) {
    // REset it and immediately return true.
    timers[key] = seconds;
    return true;
  }

  return false;
}

function cleanProperties(before) {
  const rv = {};
  for (let key in before) {
    rv[key] = before[key].value;
  }
  return rv;
}

runUnitTests('Behavior', {
  testCleanProperties: function () {
    const rawProps = { one: { value: 1 }, two: { value: 2 } };
    const cleanProps = cleanProperties(rawProps);
    assert(cleanProps.one == 1);
    assert(cleanProps.two == 2);
  },
  testQueue: function () {
    const q = new Queue();
    q.enqueue('foo');
    q.enqueue('bar');
    assert(q.peek() == 'foo');
    assert(q.dequeue() == 'foo');
    assert(q.peek() == 'bar');
    q.enqueue('baz');
    assert(q.peek() == 'bar');
    assert(q.dequeue() == 'bar');
    assert(q.peek() == 'baz');
    assert(q.dequeue() == 'baz');
  }
});

// ESM exports
export { getActorBoolean };
export { setActorBoolean };
export { getActorFloat };
export { setActorFloat };
export { getActorVector3 };
export { setActorVector3 };
export { getActorQuaternion };
export { setActorQuaternion };
export { getActorString };
export { setActorString };
export { callVoosService };
export { sysLog };
export { TIMERS_MEMORY_KEY };
export { cachedPlayerActors };
export { cleanProperties };
export { currDeltaSeconds };
export { enqueueRemoteMessage };
export { everySeconds };
export { getActorColor };
export { getPlayerActorsCached };
export { initBehaviorsPending };
export { kick };
export { moduleBehaviors_ };
export { postMessageFlush };
export { queueMessageToUnity };
export { response };
export { setActorColor };
export { tickWorld };
export { updateAgent };
export { updateBehaviorDatabase };
export { updateCount };