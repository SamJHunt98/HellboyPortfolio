using System;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;
namespace UATitle.ProcGen
{
	//grid consisting of min/max points and all the valid positions for trees within those bounds
	public struct ValidTreeGrid
	{
		public List<Vector2> validPositions;
		public Vector2 min;
		public Vector2 max;
	}
	public class ProcGenDressTrees
	{
		public void PlaceTrees(Terrain terrain, List<GameObject> treePrefab, List<ValidTreeGrid> validPositions, UARandom.Random seededRandom, Vector2 treeMinMaxSpacing, int randomMax)
		{
			TreePrototype[] treePrototypeCollection = new TreePrototype[treePrefab.Count];
			Vector3 treePos;
			for (int i = 0; i < treePrefab.Count; i++)
			{
				TreePrototype treePrototype = new TreePrototype();
				treePrototype.prefab = treePrefab[i];
				treePrototypeCollection[i] = treePrototype;
			}
			int totalTrees = (int)(terrain.terrainData.size.x / treeMinMaxSpacing.y);
			terrain.terrainData.treePrototypes = treePrototypeCollection;
			TreeInstance[] treeArray = new TreeInstance[validPositions.Count];
			List<int> weights = SetIndexWeights(treePrefab);

			List<Vector2> allPoints = PlaceTreesUsingSeed(seededRandom, validPositions, randomMax);
			for (int i = 0; i < allPoints.Count; i++)
			{
				int randomIndex = GetIndexWeight(weights, seededRandom);
				treePos = GetTreePosFromHeightmap(terrain, allPoints[i]);
				DressObjectTree treeData = treePrefab[randomIndex].GetComponent<DressObjectTree>();
				if (treeData)
				{
					TreeInstance tree = new TreeInstance()
					{
						color = Color.white,
						heightScale = 1,
						lightmapColor = Color.white,
						position = treePos,
						prototypeIndex = randomIndex,
						rotation = treeData.GetRotation(),
						widthScale = 1
					};
					treeArray[i] = tree;
				}
				else
				{
					TreeInstance tree = new TreeInstance()
					{
						color = Color.white,
						heightScale = 1,
						lightmapColor = Color.white,
						position = treePos,
						prototypeIndex = randomIndex,
						rotation = 90f,
						widthScale = 1
					};
					treeArray[i] = tree;
				}
			}
			terrain.terrainData.SetTreeInstances(treeArray, true);
		}

		Vector3 GetTreePosFromHeightmap(Terrain terrain, Vector2 position)
		{
			float xPos = position.x;
			float zPos = position.y;

			xPos *= (terrain.terrainData.size.x / terrain.terrainData.heightmapResolution);
			zPos *= (terrain.terrainData.size.z / terrain.terrainData.heightmapResolution);
			Vector3 newPosition = new Vector3(xPos / terrain.terrainData.size.x, 0, zPos / terrain.terrainData.size.z);
			return newPosition;
		}

		List<int> SetIndexWeights(List<GameObject> prefabs)
		{
			List<int> indexList = new List<int>();
			for (int i = 0; i < prefabs.Count; i++)
			{
				DressObjectTree treeData = prefabs[i].GetComponent<DressObjectTree>();
				if (treeData)
				{
					if (treeData.Weight != 0)
					{
						for (int w = 1; w <= treeData.Weight; w++)
						{
							indexList.Add(i);
						}
					}
				}
			}
			return indexList;
		}

		int GetIndexWeight(List<int> weights, UARandom.Random seededRandom)
		{
			int weightIndex = seededRandom.Next(0, weights.Count);
			return weights[weightIndex];
		}

		List<Vector2> PlaceTreesUsingSeed(UARandom.Random seededRandom, List<ValidTreeGrid> treeGrids, int randomMax)
		{
			//var timer = new Stopwatch();
			//timer.Start();
			List<Vector2> treesToPlace = new List<Vector2>();
			for (int i = 0; i < treeGrids.Count; i++)
			{
				for (int p = 0; p < treeGrids[i].validPositions.Count; p++)
				{
					int randomChance = seededRandom.Next(0, randomMax); //chance of each point being picked is relative to the size of the grid it is in
					if (randomChance == 0)
					{
						treesToPlace.Add(treeGrids[i].validPositions[p]);
						break;
					}
				}
			}
			//timer.Stop();
			//UnityEngine.Debug.Log("Function PlaceTreesUsingSeed: " + timer.Elapsed);
			return treesToPlace;
		}
	}
}


