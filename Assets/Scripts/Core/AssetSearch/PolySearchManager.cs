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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class PolySearchManager : MonoBehaviour
{
  const int MAX_ASSETS_RETURNED = 20;
  const float SCALE_COEFFICIENT = 3;
  AssetCache assetCache;

  public static string TerrainBlockHashtag = "#GBTerrainBlock";
  public static string NoAutoFitHashtag = "#GBNoAutoFit";
  public static string PointFilterHashtag = "#GBPointFilter";

  void Awake()
  {
    Util.FindIfNotSet(this, ref assetCache);
  }

  public void RequestRenderable(string uri, RenderableRequestEventHandler requestCallback)
  {
    assetCache.Get(uri, (entry) => requestCallback(entry.GetAssetClone()));
  }

  public void Search(string searchstring, OnActorableSearchResult resultCallback, System.Action<bool> onComplete)
  {
    // Poly search is disabled.
    onComplete?.Invoke(false);
  }

  public void DefaultSearch(OnActorableSearchResult resultCallback)
  {
    // Poly search is disabled.
  }

  public void RequestResultByID(string polyID, OnActorableSearchResult resultCallback)
  {
    // Poly search is disabled.
  }
}

public interface AssetSearchManager
{
  void RequestRenderable(ActorableSearchResult _requestedResult, RenderableRequestEventHandler requestCallback, int index);
  void Search(string searchstring, OnActorableSearchResult resultCallback);
  void CancelSearch();
}