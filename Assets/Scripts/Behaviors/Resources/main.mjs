import * as three from "three.mjs";

globalThis.THREE = three;
//const require = puer.module.createRequire('');

//globalThis.THREE = require("three.cjs");

const coreScripts = [
  "debug.js.txt",
  "testing.js.txt",
  "util.js.txt",
  "math.js.txt",
  "Queue.src.js.txt",
  "threejs-overrides.js.txt",
  "sleep.js.txt",
  "serialization.js.txt",
  "HandlingActor.js.txt",
  "ModuleBehaviorDatabase.js.txt",
  "HandlerApi.js.txt",
  "ModuleBehaviorsActor.js.txt",
  "ModuleBehaviorSystem.js.txt",
  "voosMain.js.txt",
  "apiv2/apiv2.js.txt",
  "apiv2/actors/actors.js.txt",
  "apiv2/actors/attributes.js.txt",
  "apiv2/actors/cloning.js.txt",
  "apiv2/actors/memory.js.txt",
  "apiv2/actors/messages.js.txt",
  "apiv2/actors/player.js.txt",
  "apiv2/actors/properties.js.txt",
  "apiv2/hierarchy/conversions.js.txt",
  "apiv2/hierarchy/parenting.js.txt",
  "apiv2/keyboard_mouse/keyboard.js.txt",
  "apiv2/keyboard_mouse/mouse.js.txt",
  "apiv2/misc/game.js.txt",
  "apiv2/misc/math.js.txt",
  "apiv2/misc/time.js.txt",
  "apiv2/multiplayer/players.js.txt",
  "apiv2/physics/attributes.js.txt",
  "apiv2/physics/casting.js.txt",
  "apiv2/physics/collisions.js.txt",
  "apiv2/physics/presets.js.txt",
  "apiv2/physics/velocity.js.txt",
  "apiv2/player_controls/actions.js.txt",
  "apiv2/player_controls/controls.js.txt",
  "apiv2/rendering/body.js.txt",
  "apiv2/rendering/color.js.txt",
  "apiv2/rendering/visibility.js.txt",
  "apiv2/terrain/blocks.js.txt",
  "apiv2/terrain/walls.js.txt",
  "apiv2/transform/position-get.js.txt",
  "apiv2/transform/position-set.js.txt",
  "apiv2/transform/rotation-get.js.txt",
  "apiv2/transform/rotation-set.js.txt",
  "apiv2/transform/scale.js.txt",
  "apiv2/player_controls/aiming.js.txt",
  "apiv2/ui/tooltips.js.txt",
  "apiv2/ui/widgets.js.txt",
  "apiv2/multiplayer/mp_events.js.txt",
  "apiv2/misc/colors.js.txt",
  "apiv2/sfx/sfx.js.txt",
  "apiv2/ui/screen.js.txt",
  "apiv2/misc/utility.js.txt",
  "apiv2/rendering/animation.js.txt",
  "apiv2/misc/cards.js.txt",
  "apiv2/particles/particleeffects.js.txt",
  "pack-unpack.js.txt",
  "apiv2/actors/camera_light.js.txt",
  "apiv2/remote/remote.js.txt",
  "apiv2/rendering/scene.js.txt"
];

// ==================== Actor Property Accessors ====================

// Boolean accessors
globalThis.getActorBoolean = function (actorId, fieldId) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  return globalThis.__voosEngine.GetActorBooleanForPuerts(actorId, fieldId);
};

globalThis.setActorBoolean = function (actorId, fieldId, value) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  globalThis.__voosEngine.SetActorBooleanForPuerts(actorId, fieldId, Boolean(value));
};

// Float accessors
globalThis.getActorFloat = function (actorId, fieldId) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  return globalThis.__voosEngine.GetActorFloatForPuerts(actorId, fieldId);
};

globalThis.setActorFloat = function (actorId, fieldId, value) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  globalThis.__voosEngine.SetActorFloatForPuerts(actorId, fieldId, Number(value));
};

// Vector3 accessors
globalThis.getActorVector3 = function (actorId, fieldId, pos) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  // C# out parameters need to be wrapped with $ref and unwrapped with $unref
  const outX = puer.$ref();
  const outY = puer.$ref();
  const outZ = puer.$ref();
  globalThis.__voosEngine.GetActorVector3ForPuerts(actorId, fieldId, outX, outY, outZ);
  pos.x = puer.$unref(outX);
  pos.y = puer.$unref(outY);
  pos.z = puer.$unref(outZ);
};

globalThis.setActorVector3 = function (actorId, fieldId, x, y, z) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }

  // Pass x, y, z as separate parameters
  globalThis.__voosEngine.SetActorVector3ForPuerts(
    actorId,
    fieldId,
    x,
    y,
    z
  );
};

// Quaternion accessors
globalThis.getActorQuaternion = function (actorId, fieldId, quaternion) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  // C# out parameters need to be wrapped with $ref and unwrapped with $unref
  const outX = puer.$ref();
  const outY = puer.$ref();
  const outZ = puer.$ref();
  const outW = puer.$ref();
  globalThis.__voosEngine.GetActorQuaternionForPuerts(actorId, fieldId, outX, outY, outZ, outW);
  
  quaternion.x = puer.$unref(outX);
  quaternion.y = puer.$unref(outY);
  quaternion.z = puer.$unref(outZ);
  quaternion.w = puer.$unref(outW);

};

globalThis.setActorQuaternion = function (actorId, fieldId, x, y, z, w) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }

  // Pass x, y, z, w as separate parameters
  globalThis.__voosEngine.SetActorQuaternionForPuerts(
    actorId,
    fieldId,
    x,
    y,
    z,
    w
  );
};

// String accessors
globalThis.getActorString = function (actorId, fieldId) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  return globalThis.__voosEngine.GetActorStringForPuerts(actorId, fieldId);
};

globalThis.setActorString = function (actorId, fieldId, value) {
  if (!globalThis.__voosEngine) {
    throw new Error('VoosEngine not registered');
  }
  globalThis.__voosEngine.SetActorStringForPuerts(actorId, fieldId, String(value));
};

// ==================== Service Call API ====================

// callVoosService - Main service call API (compatible with V8 engine)
// Note: This is a synchronous-looking API but internally uses async callback
// It's kept for backward compatibility with existing code
globalThis.callVoosService = function (serviceName, arg) {
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

// log - Logging API
globalThis.log = function (...args) {
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

// sysLog - System logging API (alias for log)
globalThis.sysLog = function (...args) {
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

// ==================== Utility Functions ====================

globalThis.assert = function (condition, message) {
  console.assert(condition, message);
};

// ==================== Initialization ====================

export function updateAgentPostMessageFlush (request, arrayBuffer) {
	  updateAgent(request, arrayBuffer);
	  postMessageFlush(request, arrayBuffer);
	  return arrayBuffer;
}

globalThis.__voosModules = {};

globalThis.getVoosModule = function (moduleName) {
  if (!globalThis.__voosModules[moduleName]) {
    throw new Error('Module not found: ' + moduleName);
  }
  return globalThis.__voosModules[moduleName];
};

export function setGlobal(k, v) {
  globalThis[k] = v;
}

export function jsonStringify(obj) {
  return JSON.stringify(obj);
}

export function jsonParse(json) {
  return JSON.parse(json);
}

console.log('[Polyfill] V8 compatibility layer loaded');
