using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPSMath : MonoBehaviour {
	// The size of a degree of latitude and longitude in meters.
	// All values taken from "The American Practical Navigator" Table 7
	// http://msi.nga.mil/NGAPortal/MSI.portal?_nfpb=true&_pageLabel=msi_portal_page_62&pubCode=0002
	// http://msi.nga.mil//MSISiteContent/StaticFiles/NAV_PUBS/APN/Tables/T-07.pdf
	// All of these values change depending on current latitude.
	// Since we start at 0 degrees and end at 90 degrees,
	// we can directly access each value without having to subtract 1.

	// latBase is the distance from the equator (0*) to
	// the floor value of the current Latitude (i.e. the sum of all prior lat lengths)
	// since all latitudes have a different distance to the next latitude
	private float[] _latBase = {0, 110574, 221149, 331725, 442302, 552882, 663465, 774051, 884642, 995238, 1105839,
		1216447, 1327062, 1437684, 1548314, 1658953, 1769602, 1880261, 1990930, 2101610, 2212302, 2323006, 2433723,
		2544453, 2655197, 2765955, 2876728, 2987516, 3098320, 3209139, 3319975, 3430827, 3541696, 3652583, 3763487,
		3874409, 3985350, 4096309, 4207287, 4318283, 4429298, 4540333, 4651387, 4762460, 4873553, 4984665, 5095797,
		5206948, 5318119, 5429309, 5540519, 5651748, 5762996, 5874263, 5985549, 6096854, 6208177, 6319519, 6430879,
		6542257, 6653652, 6765064, 6876493, 6987939, 7099401, 7210878, 7322371, 7433878, 7545400, 7656936, 7768485,
		7880047, 7991621, 8103207, 8214804, 8326412, 8438030, 8549658, 8661295, 8772940, 8884593, 8996253, 9107919,
		9219591, 9331268, 9442950, 9554635, 9666323, 9778014, 9889707, 10001401};

	// latFactor is the length of a degree of latitude at the floor value of the current latitude
	private float[] _latFactor = {110574, 110575, 110576, 110577, 110580, 110583, 110586, 110591, 110596, 110601,
		110608, 110615, 110622, 110630, 110639, 110649, 110659, 110669, 110680, 110692, 110704, 110717, 110730,
		110744, 110758, 110773, 110788, 110804, 110819, 110836, 110852, 110869, 110887, 110904, 110922, 110941,
		110959, 110978, 110996, 111015, 111035, 111054, 111073, 111093, 111112, 111132, 111151, 111171, 111190,
		111210, 111229, 111248, 111267, 111286, 111305, 111323, 111342, 111360, 111378, 111395, 111412, 111429,
		111446, 111462, 111477, 111493, 111507, 111522, 111536, 111549, 111562, 111574, 111586, 111597, 111608,
		111618, 111628, 111637, 111645, 111653, 111660, 111666, 111672, 111677, 111682, 111685, 111688, 111691,
		111693, 111694, 0};

	// lonFactor is the length of a degree of longitude at the floor value of the current latitude (yes, latitude)
	private float[] _lonFactor = {111319, 111303, 111252, 111168, 111050, 110899, 110714, 110495, 110243, 109958,
		109639, 109288, 108903, 108485, 108034, 107550, 107034, 106486, 105905, 105292, 104647, 103970, 103262,
		102523, 101752, 100950, 100118, 99255, 98362, 97439, 96486, 95504, 94493, 93453, 92385, 91288, 90164,
		89012, 87832, 86626, 85394, 84135, 82851, 81541, 80206, 78847, 77463, 76056, 74625, 73172, 71696, 70198,
		68678, 67137, 65576, 63994, 62393, 60772, 59133, 57475, 55800, 54107, 52398, 50673, 48932, 47176, 45405,
		43620, 41822, 40010, 38187, 36351, 34504, 32647, 30779, 28902, 27016, 25121, 23219, 21310, 19393, 17471,
		15544, 13611, 11675, 9735, 7791, 5846, 3898, 1949, 0};

	public Vector2 _worldOriginLatLong;

	private double _originLatInMeters;

    private void Awake() {
		PerformInitialization();
	}

	// NOTE: If you change the worldOriginLatLong at runtime, you need to rerun this function.
	public void PerformInitialization() {
		_originLatInMeters = (double)GetLatBaseInMeters(_worldOriginLatLong.x) +
			((double)GetLatHeightInMeters(_worldOriginLatLong.x) * ((double)_worldOriginLatLong.x % 1.0));
	}

    // We can directly calculate the latitude by multiplying the size of the current latitude by
    // the percentage we've gone into the current latitude and
    // adding the base value for the current latitude (distance from the equator).
    // (sizeOfLatInM * (currentLat % 1)) + latBaseInM

    // (int) Mathf.Abs(latitude) calculates which whole number degree of latitude we're in.
    // Abs(lat) makes sure the value is positive and (int) removes all decimal places like floor.
    public float GetLatHeightInMeters(float l) {
		return _latFactor[(int)Mathf.Abs(l)];
	}

	public float GetLatBaseInMeters(float l) {
		if (l < 0) {
			return -_latBase[(int)Mathf.Abs(l)];
		} else {
			return _latBase[(int)Mathf.Abs(l)];
		}
	}

	//Calculating the longitude is even easier than latitude: (currentLon * lonInM)
	public float GetLonWidthInMeters(float l) {
		int latFloor = (int)Mathf.Abs(l); // What whole number degree of latitude are we in?
										  // If Lat is equal to 90, we're at the exact top or bottom of the world.
										  // There are no more latitudes beyond this one.
		if (latFloor >= 90) {
			// Circumference of the latititude at the poles is 0 meters.
			// Thus each degree of longitude is also 0 meters in length
			return 0;
		} else {
			// Lerp between floor and ceiling lengths of degree of longitude
			return Mathf.Lerp(_lonFactor[latFloor], _lonFactor[latFloor + 1], Mathf.Abs(l % 1));
		}
	}

	// TODO: Use a binary search instead of checking linearly.
	// We only have to check up to 90 values so not too slow, but could definitely be optimized.
	public float GetLatBaseFromLatInMeters(double l) {
		for (int i = 1; i < 91; i++) {
			if (l < _latBase[i]) {
				return i - 1;
			}
		}
		return 90;
	}

	// Takes current position in the world in meters and returns GPS coords.
	// Useful for if your object has been moved about manually or by physics engine,
	// and you need to know where it is in the real world.
	public Vector2 CalculateLatLonFromObjectPosition(Vector3 objectPosition) {
		double objectLatInMeters = _originLatInMeters + objectPosition.z;
		float latBase = GetLatBaseFromLatInMeters(objectLatInMeters);
		float rawLat = latBase + (float)((objectLatInMeters - GetLatBaseInMeters(latBase)) /
			GetLatHeightInMeters(latBase));
		float rawLon = _worldOriginLatLong[1] + (objectPosition.x / GetLonWidthInMeters(rawLat));

		return new Vector2(rawLat, rawLon);
	}

	public Vector2 WorldOriginLatLong {
		set {
			_worldOriginLatLong = value;
			PerformInitialization();
		}
    }
}
