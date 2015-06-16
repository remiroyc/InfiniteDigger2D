using UnityEngine;
using System.Collections;

public static class TerrainFactory {


	public static string[] GetTerrain(){

		string[] tab;
		var randomNumber = Random.value;

		if(randomNumber <= 0.33f){
		
			tab = new string[9];
			tab[0] = "      AAA      ";
			tab[1] = "     AAAAA     ";
			tab[2] = "    AAAAAAA    ";
			tab[3] = "   ACCCCCCCA   ";
			tab[4] = "  AAACCCCCAAA  ";
			tab[5] = " AAAAACCCAAAAA ";
			tab[6] = "AAAAAAACAAAAAAA";
			tab[7] = "BBBBBBBBBBBBBBB";
			tab[7] = "BBBBBBBBBBBBBBB";

		}else if(randomNumber <= 0.66f){

			tab = new string[8];
			tab[0] = "           ";
			tab[1] = " AAAAAA    ";
			tab[2] = "AAAAAAAA   ";
			tab[3] = "AA  A  AA  ";
			tab[4] = "AA  A  AA  ";
			tab[5] = " AAA AAA   ";
			tab[6] = "  AAAAA    ";
			tab[7] = "           ";

		}else{

			tab = new string[8];
			tab[0] = " B A B A B A";
			tab[1] = "B A B A B A ";
			tab[2] = " A B A B A B";
			tab[3] = "A B A B A B ";
			tab[4] = " B A B A B A";
			tab[5] = "B A B A B A ";
			tab[6] = " A B A B A B";
			tab[7] = "A B A B A B ";
		}

		return tab;

	}

}
