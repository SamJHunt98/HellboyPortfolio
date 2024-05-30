using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//a compilation of excerpts of code relevant to the Tree Generation from amongst the greater ProcGen Terrain file.
public class ProcGenDressTerrainExcerpt
{
    //using a data variable to set how far from the level we consider out of bounds to be for the sake of spawning trees
    float checkDistance = dressData.OutOfBoundsStartDistance;
    if (dressData.UseTrees)
	{
	    if (dressData.BackgroundTreesStartDistance > checkDistance)
		{
			checkDistance = dressData.BackgroundTreesStartDistance;
		}
	}

    //...

    //Calculate an array of all coordinates on the terrain and how far they are from the nearest point in the level
    int stepSize = (int)(dressData.HeightmapResolutionPerMeter * checkDistance);
	int xsteps = (maxCoord.x - minCoord.x) / stepSize + 1;
	int ysteps = (maxCoord.y - minCoord.y) / stepSize + 1;
	for (int sx = 0; sx < xsteps; ++sx)
	{
		for (int sy = 0; sy < ysteps; ++sy)
		{
			// For this mega square, check corners, if all outside range fill with maxfloat else calculate properly
			Vector2Int tl = minCoord; //top left
			tl.x += sx * stepSize;
			tl.y += sy * stepSize;
			Vector2Int tr = tl; //top right
			tr.x = Mathf.Min(tr.x + stepSize, maxCoord.x);

			Vector2Int bl = tl; //bottom left
			bl.y = Mathf.Min(bl.x + stepSize, maxCoord.y);

			Vector2Int br = tr; //bottom right
			br.y = Mathf.Min(br.y + stepSize, maxCoord.y);

			var tlPos = heightfieldPositions[tl.x, tl.y];
			var tlClosest = outlineCalculator.GetClosestPointToHeightfieldCoordinate(tl, heightfieldPositions[tl.x, tl.y], checkDistance);
			var tlDist = Vector2.Distance(tlClosest, new Vector2(tlPos.x, tlPos.z));

			var trPos = heightfieldPositions[tr.x, tr.y];
			var trClosest = outlineCalculator.GetClosestPointToHeightfieldCoordinate(tr, heightfieldPositions[tr.x, tr.y], checkDistance);
			var trDist = Vector2.Distance(trClosest, new Vector2(trPos.x, trPos.z));

			var blPos = heightfieldPositions[bl.x, bl.y];
			var blClosest = outlineCalculator.GetClosestPointToHeightfieldCoordinate(bl, heightfieldPositions[bl.x, bl.y], checkDistance);
			var blDist = Vector2.Distance(blClosest, new Vector2(blPos.x, blPos.z));

			var brPos = heightfieldPositions[br.x, br.y];
			var brClosest = outlineCalculator.GetClosestPointToHeightfieldCoordinate(br, heightfieldPositions[br.x, br.y], checkDistance);
			var brDist = Vector2.Distance(brClosest, new Vector2(brPos.x, brPos.z));

			if (tlDist > checkDistance && trDist > checkDistance && brDist > checkDistance && brDist > checkDistance)
			{
				// Fill square with max
				for (int x = tl.x; x < br.x; ++x)
				{
					for (int y = tl.y; y < br.y; ++y)
					{
						heightfieldDistances[x, y] = float.MaxValue;
					}
				}
			}
			else
			{
				for (int x = tl.x; x < br.x; ++x)
				{
					for (int y = tl.y; y < br.y; ++y)
					{
						var pos = outlineCalculator.GetClosestPointToHeightfieldCoordinate(new Vector2Int(x, y), heightfieldPositions[x, y], checkDistance);
						heightfieldDistances[x, y] = Vector2.Distance(pos, new Vector2(heightfieldPositions[x, y].x, heightfieldPositions[x,y].z));
					}
				}
			}
		}

		yield return null;
	}

    //...

    //create a set of grid squares and check whether all the points within are out of bounds, then set the values to reflect this
    List<ValidTreeGrid> validTreeGrids = new List<ValidTreeGrid>();

	if (dressData.UseTrees)
	{
		int treeStepSize = (int)(dressData.HeightmapResolutionPerMeter * dressData.BackgroundTreesStartDistance);
		for (int x = 0; x < heightmapResolution; x += treeStepSize)
		{
			for (int y = 0; y < heightmapResolution; y += treeStepSize)
			{
				float distance = heightfieldDistances[x, y];
				int lx = x > 0 ? x - treeStepSize : x;
				int ly = y > 0 ? y - treeStepSize : y;
				float lastDistance = heightfieldDistances[lx, ly];
				if (distance > dressData.BackgroundTreesStartDistance && lastDistance > dressData.BackgroundTreesStartDistance) //check that all points in the square will be out of bounds
				{
					List<Vector2> valid = new List<Vector2>();
					for (int px = lx + 1; px <= x; px++)
					{
						for (int py = ly + 1; py <= y; py++)
						{
							valid.Add(new Vector2(px, py));
						}
					}
					ValidTreeGrid grid = new ValidTreeGrid();
					grid.validPositions = valid;
					grid.min = new Vector2(lx + 1, ly + 1);
					grid.max = new Vector2(x, y);
					validTreeGrids.Add(grid);
				}
			}
		}
		terrain.treeDistance = dressData.TreeRenderDistance;
		terrain.treeMaximumFullLODCount = dressData.MaxRenderedTrees;
		terrain.treeBillboardDistance = dressData.TreeBillboardDistance;
		terrain.treeCrossFadeLength = dressData.TreeCrossFade;
	}

    //...

    //call the PlaceTrees function now that we have the correct values to use
    if (dressData.UseTrees)
	{
		treeGenerator.PlaceTrees(terrain, dressData.TreePrefabs, validTreeGrids, seededRandom, dressData.SpacingAndDensity, dressData.TreeRandomisationMax);
	}
}

