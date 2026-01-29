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
  propCardTargetActor('Target', {
    label: 'Whose variable?'
  }),
  propString('VarName', 'MyVar'),
  propNumber('X', 1000),
  propNumber('Y', 100),
  propString('Label', 'My var'),

  propBoolean('AutoSize', true),
  propNumber('Width', 40, { requires: requireFalse('AutoSize') }),
  propNumber('Height', 40, { requires: requireFalse('AutoSize') }),

  propColor('LabelColor', '#ffff00'),
  propNumber('LabelSize', 30),
  propEnum('LabelAlign', 'LEFT', ['LEFT', 'RIGHT', 'CENTER']),

  propColor('ValueColor', '#ffffff'),
  propNumber('ValueSize', 40),
  propEnum('ValueAlign', 'LEFT', ['LEFT', 'RIGHT', 'CENTER']),

  propBoolean('HasBackground', true),
  propColor('BackgroundColor', '#000020', { requires: requireTrue('HasBackground') }),
  propDecimal('Opacity', 0.7),
  propNumber('Padding', 10),
]

const SPACING_BETWEEN_LABEL_AND_VALUE = 5;

export function getCardStatus() {
  return {
    description: `Show variable <color=yellow>${getProps().VarName}</color> on screen at <color=green>(${getProps().X}, ${getProps().Y})`
  }
}

export function onDrawScreen() {
  const boxWidth = calcWidth();
  const boxHeight = calcHeight();
  if (getProps().HasBackground) {
    uiRect(getProps().X, getProps().Y, boxWidth, boxHeight, getProps().BackgroundColor, { opacity: getProps().Opacity });
  }
  drawAlignedText(
    getProps().X + getProps().Padding,
    getProps().Y + getProps().Padding,
    boxWidth - 2 * getProps().Padding,
    getProps().Label,
    getProps().LabelColor,
    getProps().LabelSize,
    getProps().LabelAlign);
  drawAlignedText(
    getProps().X + getProps().Padding,
    getProps().Y + getProps().Padding + SPACING_BETWEEN_LABEL_AND_VALUE + uiGetTextHeight(getProps().Label, getProps().LabelSize),
    boxWidth - 2 * getProps().Padding,
    getVarValue(),
    getProps().ValueColor,
    getProps().ValueSize,
    getProps().ValueAlign);
}

function calcWidth() {
  const val = getVarValue();
  if (getProps().AutoSize) {
    return 2 * getProps().Padding + Math.max(uiGetTextWidth(getProps().Label, getProps().LabelSize), uiGetTextWidth(val, getProps().ValueSize));
  } else {
    return getProps().Width;
  }
}

function calcHeight() {
  const val = getVarValue();
  if (getProps().AutoSize) {
    return uiGetTextHeight(getProps().Label, getProps().LabelSize) + uiGetTextHeight(val, getProps().ValueSize) + SPACING_BETWEEN_LABEL_AND_VALUE + 2 * getProps().Padding;
  } else {
    return getProps().Height;
  }
}

function getVarValue() {
  const actor = getCardTargetActor('Target');
  return exists(actor) ? ("" + getVar(getProps().VarName, actor)) : '?';
}

function drawAlignedText(x, y, width, text, color, textSize, align) {
  if (align === 'RIGHT') {
    x = x + width - uiGetTextWidth(text, textSize);
  } else if (align === 'CENTER') {
    x += 0.5 * (width - uiGetTextWidth(text, textSize));
  }
  uiText(x, y, text, color, { textSize: textSize });
}
