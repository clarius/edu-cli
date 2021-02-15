using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Clarius.Edu.Graph
{
    public static class SchoolManager
    {
        static List<Section> sections = new List<Section>();

        static List<string> allGrades = new List<string> { Constants.GRADE_FIRST, Constants.GRADE_SECOND, Constants.GRADE_THIRD, Constants.GRADE_FOURTH, Constants.GRADE_FIFTH, Constants.GRADE_SIXTH };
        static List<string> allDivisions = new List<string> { Constants.DIVISION_A, Constants.DIVISION_B, Constants.DIVISION_C };
        static List<string> allLevels = new List<string> { Constants.LEVEL_INICIAL, Constants.LEVEL_PRIMARIA, Constants.LEVEL_SECUNDARIA };
        static List<string> allEnglishLevels = new List<string> { Constants.ENGLISH_LEVEL_1, Constants.ENGLISH_LEVEL_2, Constants.ENGLISH_LEVEL_3 };
        static List<string> grades123 = new List<string> { Constants.GRADE_FIRST, Constants.GRADE_THIRD, Constants.GRADE_FIFTH };
        static List<string> grades246 = new List<string> { Constants.GRADE_SECOND, Constants.GRADE_FOURTH, Constants.GRADE_SIXTH };
        static List<string> grades456 = new List<string> { Constants.GRADE_FOURTH, Constants.GRADE_FIFTH, Constants.GRADE_SIXTH };
        static List<string> allUserTypes = new List<string> { Constants.USER_TYPE_STUDENT, Constants.USER_TYPE_TEACHER, Constants.USER_TYPE_TEST, Constants.USER_TYPE_PRECEPTOR, Constants.USER_TYPE_IT, Constants.USER_TYPE_FUNDACION, Constants.USER_TYPE_EGRESADO };
        static List<string> allGroupTypes = new List<string> { Constants.GROUP_TYPE_CLASS, Constants.USER_TYPE_TEACHER };

        static SchoolManager()
        {
            // Inicial level
            sections.Add(new Section("Amarilla {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("Amarilla Taller de Artes Visuales {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("Amarilla Teatro {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("Amarilla Catequesis {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("Amarilla Musica {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("Amarilla Inglés {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("Amarilla Informatica {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("Amarilla EOE {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("Amarilla Ed. Fisica {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("Verde {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_SECOND }, allDivisions));
            sections.Add(new Section("Verde Taller de Artes Visuales {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_SECOND }, allDivisions));
            sections.Add(new Section("Verde Teatro {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_SECOND }, allDivisions));
            sections.Add(new Section("Verde Catequesis {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_SECOND }, allDivisions));
            sections.Add(new Section("Verde Musica {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_SECOND }, allDivisions));
            sections.Add(new Section("Verde Inglés {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_SECOND }, allDivisions));
            sections.Add(new Section("Verde Informatica {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_SECOND }, allDivisions));
            sections.Add(new Section("Verde EOE {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_SECOND }, allDivisions));
            sections.Add(new Section("Verde Ed. Fisica {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_SECOND }, allDivisions));
            sections.Add(new Section("Azul {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Azul Taller de Artes Visuales {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Azul Teatro {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Azul Catequesis {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Azul Musica {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Azul Inglés {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Azul Informatica {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Azul Ed. Fisica {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Azul EOE {0} {1}", Constants.LEVEL_INICIAL, new List<string> { Constants.GRADE_THIRD }, allDivisions));


            // Primaria level
            sections.Add(new Section("Computación {0} {1}", Constants.LEVEL_PRIMARIA, allGrades, allDivisions));
            sections.Add(new Section("Plástica {0} {1}", Constants.LEVEL_PRIMARIA, grades123, allDivisions));
            sections.Add(new Section("Música {0} {1}", Constants.LEVEL_PRIMARIA, grades246, allDivisions));
            sections.Add(new Section("Matemática {0} {1}", Constants.LEVEL_PRIMARIA, grades456, allDivisions));
            sections.Add(new Section("Inglés {0} {1}", Constants.LEVEL_PRIMARIA, grades123, allDivisions));
            sections.Add(new Section("Inglés {0} {1}", Constants.LEVEL_PRIMARIA, grades456, allEnglishLevels));
            sections.Add(new Section("{0} Grado {1}", Constants.LEVEL_PRIMARIA, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("{0} Grado {1}", Constants.LEVEL_PRIMARIA, new List<string> { Constants.GRADE_SECOND }, allDivisions));
            sections.Add(new Section("{0} Grado {1}", Constants.LEVEL_PRIMARIA, new List<string> { Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("P. del Lenguaje {0} {1}", Constants.LEVEL_PRIMARIA, grades456, allDivisions));
            sections.Add(new Section("Educación Física {0} {1}", Constants.LEVEL_PRIMARIA, allGrades, allDivisions));
            sections.Add(new Section("Cs. Naturales / Taller {0} {1}", Constants.LEVEL_PRIMARIA, grades456, allDivisions));
            sections.Add(new Section("Ciencias Sociales {0} {1}", Constants.LEVEL_PRIMARIA, grades456, allDivisions));
            sections.Add(new Section("Catequesis {0} {1}", Constants.LEVEL_PRIMARIA, allGrades, allDivisions));

            // Secundaria level
            sections.Add(new Section("Antropología {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FOURTH }, allDivisions));
            sections.Add(new Section("Arte {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_SIXTH }, allDivisions));
            sections.Add(new Section("Biología {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_SECOND, Constants.GRADE_THIRD, Constants.GRADE_FOURTH }, allDivisions));
            sections.Add(new Section("Ciencias Naturales {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("Ciencias Sociales {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("Comunicación C y S {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIFTH }, allDivisions));
            sections.Add(new Section("Construcción de la Ciudadanía {0} {1} Ambiente", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIRST, Constants.GRADE_SECOND, Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Construcción de la Ciudadanía {0} {1} Democrática", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIRST, Constants.GRADE_SECOND, Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Construcción de la Ciudadanía {0} {1} Digital", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIRST, Constants.GRADE_SECOND, Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Economía política {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIFTH   }, allDivisions));
            sections.Add(new Section("Educación Física {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIRST, Constants.GRADE_SECOND, Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Educación Física {0} {1} Fasano", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FOURTH, Constants.GRADE_SIXTH }, allDivisions));
            sections.Add(new Section("Educación Física {0} {1} Gismano", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FOURTH, Constants.GRADE_FIFTH, Constants.GRADE_SIXTH }, allDivisions));
            sections.Add(new Section("Educación Física {0} {1} Timko", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FOURTH, Constants.GRADE_SIXTH }, allDivisions));
            sections.Add(new Section("Educación Física {0} {1} Haramboure", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIFTH }, allDivisions));
            sections.Add(new Section("Educación Física {0} {1} Cuenca", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIFTH }, allDivisions)); // TODO IJME: Cueca appears to have only B and C divisions (missing A?)
            sections.Add(new Section("Educación Artística {0} {1}", Constants.LEVEL_SECUNDARIA, grades123, allDivisions));
            sections.Add(new Section("Espacio de orientación  {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIFTH, Constants.GRADE_SIXTH }, allDivisions));
            sections.Add(new Section("Filosofía {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_SIXTH }, allDivisions));
            sections.Add(new Section("Física {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FOURTH }, allDivisions));
            sections.Add(new Section("Fisico Química {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_SECOND, Constants.GRADE_THIRD }, allDivisions)); // TODO IJME: ijme is missing 2do A of this section   
            sections.Add(new Section("Geografía {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_SECOND, Constants.GRADE_THIRD, Constants.GRADE_FOURTH, Constants.GRADE_FIFTH, Constants.GRADE_SIXTH }, allDivisions));
            sections.Add(new Section("Historia {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_SECOND, Constants.GRADE_THIRD, Constants.GRADE_FOURTH, Constants.GRADE_FIFTH, Constants.GRADE_SIXTH }, allDivisions));
            sections.Add(new Section("Inglés {0} {1}", Constants.LEVEL_SECUNDARIA, allGrades, allEnglishLevels));
            sections.Add(new Section("Introducción a la química {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIFTH }, allDivisions));
            sections.Add(new Section("Literatura {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FOURTH, Constants.GRADE_FIFTH, Constants.GRADE_SIXTH }, allDivisions));
            sections.Add(new Section("Lógica {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FOURTH }, allDivisions));
            sections.Add(new Section("Matemática {0} {1}", Constants.LEVEL_SECUNDARIA, allGrades, allDivisions));
            sections.Add(new Section("MEP {0} {1} Ambiente", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("MEP {0} {1} Comunicación", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("MEP {0} {1} Sociales", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("MIC {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIFTH }, allDivisions));
            sections.Add(new Section("NTICX {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FOURTH }, allDivisions));
            sections.Add(new Section("Política y Ciudadanía {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIFTH }, allDivisions));
            sections.Add(new Section("Prácticas del lenguaje {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIRST, Constants.GRADE_SECOND, Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Proyecto de investigación {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_SIXTH }, allDivisions));
            sections.Add(new Section("Psicología {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FOURTH }, allDivisions));
            sections.Add(new Section("Salud y adolescencia {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FOURTH }, allDivisions));
            sections.Add(new Section("Sociología {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIFTH }, allDivisions));
            sections.Add(new Section("Teología {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_SECOND, Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("Trabajo y ciudadanía {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_SIXTH }, allDivisions));
            sections.Add(new Section("Tutorías-Afectividad {0} {1}", Constants.LEVEL_SECUNDARIA, new List<string> { Constants.GRADE_FIRST, Constants.GRADE_SECOND, Constants.GRADE_THIRD }, allDivisions));
            // TODO IJME: missing ed. fisica 5to A? (there are 2, should be 3, one per trimester)
        }

        static public IEnumerable<Section> GetSections(string level)
        {
            return sections.Where(p => string.Equals(level, p.Level, StringComparison.InvariantCultureIgnoreCase));
        }

        public static List<string> UserTypes
        {
            get { return allUserTypes; }
        }
        public static List<string> GroupTypes
        {
            get { return allGroupTypes; }
        }

        public static List<string> Grades
        {
            get { return allGrades; }
        }

        public static List<string> Divisions
        {
            get { return allDivisions; }
        }
        public static List<string> EnglishLevels
        {
            get { return allEnglishLevels; }
        }
        public static List<string> Levels
        {
            get { return allLevels; }
        }

        public static bool GetPromotedGradeAndLevel(string currentGrade, string currentLevel, out string newGrade, out string newLevel)
        {
            if (IsLastLevel(currentLevel) && IsLastGradeOfLevel(currentGrade, currentLevel))
            {
                // there is nowhere to promote this current grade/
                newGrade = null; newLevel = null;
                return false;
            }

            newGrade = newLevel = null;

            for (int c = 0; c < allGrades.Count; c++)
            {
                if (string.Equals(allGrades[c], currentGrade, StringComparison.InvariantCultureIgnoreCase))
                {
                    // if the current grade happens to be the last grade of the current level, we need to bump it to first grade, next level
                    if (string.Equals(allGrades[c], GetLastGradeOfLevel(currentLevel), StringComparison.InvariantCultureIgnoreCase))
                    {
                        newLevel = GetNextLevel(currentLevel);
                        newGrade = GetFirstGradeOfLevel(newLevel);
                    }
                    else
                    {
                        newGrade = allGrades[c + 1];
                        newLevel = currentLevel;
                    }
                    return true;
                }
            }
            // no grade matching current grade? this is bad...
            throw new ArgumentException("Specified currentGrade is not valid");
        }

        static bool IsLastGradeOfLevel(string currentGrade, string currentLevel)
        {
            if (string.Equals(GetLastGradeOfLevel(currentLevel), currentGrade, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        static bool IsLastLevel(string level)
        {
            if (string.Equals(level, Constants.LEVEL_SECUNDARIA, StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }
        static string GetFirstGradeOfLevel(string level)
        {
            if (string.Equals(Constants.LEVEL_INICIAL, level, StringComparison.InvariantCultureIgnoreCase))
            {
                return ""; // TODO: ?? what should go here ??
            }

            if (string.Equals(Constants.LEVEL_PRIMARIA, level, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(Constants.LEVEL_SECUNDARIA, level, StringComparison.InvariantCultureIgnoreCase))
            {
                return "1ero";
            }

            throw new ArgumentException("Invalid value for level");
        }
        static string GetLastGradeOfLevel(string level)
        {
            if (string.Equals(Constants.LEVEL_INICIAL, level, StringComparison.InvariantCultureIgnoreCase))
            {
                return ""; // TODO: ?? what should go here ??
            }

            if (string.Equals(Constants.LEVEL_PRIMARIA, level, StringComparison.InvariantCultureIgnoreCase) ||
                string.Equals(Constants.LEVEL_PRIMARIA, level, StringComparison.InvariantCultureIgnoreCase))
            {
                return Constants.GRADE_SIXTH;
            }

            throw new ArgumentException("Invalid value for level");
        }

        static string GetNextLevel(string currentLevel)
        {
            switch (currentLevel)
            {
                case Constants.LEVEL_INICIAL:
                    return Constants.LEVEL_PRIMARIA;
                case Constants.LEVEL_PRIMARIA:
                    return Constants.LEVEL_SECUNDARIA;
            }

            return null;
        }
    }

    public class Section
    {
        public string Id { get; }
        public string Level { get; }
        public List<string> Grades { get; }
        public List<string> Divisions { get; }

        public Section(string id, string level, List<string> grades, List<string> divisions)
        {
            this.Id = id;
            this.Level = level;
            this.Grades = grades;
            this.Divisions = divisions;
        }

        public List<string> GetNames()
        {
            var names = new List<string>();

            foreach (var grade in Grades)
            {
                foreach (var division in Divisions)
                {
                    names.Add(string.Format(Id, grade, division));
                }
            }

            return names;
        }


    }




}
