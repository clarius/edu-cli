using System;
using System.Collections.Generic;

namespace Clarius.Edu.Graph
{
    public static class SchoolManager
    {
        static List<Section> sections = new List<Section>();

        static List<string> allGrades = new List<string> { Constants.GRADE_FIRST, Constants.GRADE_SECOND, Constants.GRADE_THIRD, Constants.GRADE_FOURTH, Constants.GRADE_FIFTH, Constants.GRADE_SIXTH };
        static List<string> allDivisions = new List<string> { Constants.DIVISION_A, Constants.DIVISION_B, Constants.DIVISION_C, Constants.ENGLISH_LEVEL_1, Constants.ENGLISH_LEVEL_2, Constants.ENGLISH_LEVEL_3 };
        static List<string> allLevels = new List<string> { Constants.LEVEL_INICIAL, Constants.LEVEL_PRIMARIA, Constants.LEVEL_SECUNDARIA };
        static List<string> allEnglishLevels = new List<string> { Constants.ENGLISH_LEVEL_1, Constants.ENGLISH_LEVEL_2, Constants.ENGLISH_LEVEL_3 };
        static List<string> grades123 = new List<string> { Constants.GRADE_FIRST, Constants.GRADE_THIRD, Constants.GRADE_FIFTH };
        static List<string> grades246 = new List<string> { Constants.GRADE_SECOND, Constants.GRADE_FOURTH, Constants.GRADE_SIXTH};
        static List<string> grades456 = new List<string> { Constants.GRADE_FOURTH, Constants.GRADE_FIFTH, Constants.GRADE_SIXTH};
        static List<string> allUserTypes = new List<string> { Constants.USER_TYPE_STUDENT, Constants.USER_TYPE_TEACHER, Constants.USER_TYPE_TEST, Constants.USER_TYPE_PRECEPTOR, Constants.USER_TYPE_IT, Constants.USER_TYPE_FUNDACION, Constants.USER_TYPE_EGRESADO };

        static SchoolManager()
        {
            sections.Add(new Section("Computación {0} {1}", Constants.LEVEL_PRIMARIA, allGrades, allDivisions));
            sections.Add(new Section("Plástica {0} {1}", Constants.LEVEL_PRIMARIA, grades123, allDivisions));
            sections.Add(new Section("Música {0} {1}", Constants.LEVEL_PRIMARIA, grades246, allDivisions));
            sections.Add(new Section("Matemática {0} {1}", Constants.LEVEL_PRIMARIA, grades456, allDivisions));
            sections.Add(new Section("Inglés {0} {1}", Constants.LEVEL_PRIMARIA, grades123, allDivisions));
            sections.Add(new Section("Inglés {0} {1}", Constants.LEVEL_PRIMARIA, grades456, allEnglishLevels));
            sections.Add(new Section("{0} Grado {1}", Constants.LEVEL_PRIMARIA, new List<string> { Constants.GRADE_FIRST }, allDivisions));
            sections.Add(new Section("{0} Grado {1}", Constants.LEVEL_PRIMARIA, new List<string> { Constants.GRADE_SECOND }, allDivisions));
            sections.Add(new Section("{0} Grado {1}", Constants.LEVEL_PRIMARIA, new List<string> { Constants.GRADE_THIRD }, allDivisions));
            sections.Add(new Section("P. del Lenguaje {0} {1}", Constants.LEVEL_PRIMARIA, grades456, allEnglishLevels));
            sections.Add(new Section("Educación Física {0} {1}", Constants.LEVEL_PRIMARIA, allGrades, allEnglishLevels));
            sections.Add(new Section("Cs. Naturales / Taller {0} {1}", Constants.LEVEL_PRIMARIA, grades456, allEnglishLevels));
            sections.Add(new Section("Ciencias Sociales {0} {1}", Constants.LEVEL_PRIMARIA, grades456, allEnglishLevels));
            sections.Add(new Section("Catequesis {0} {1}", Constants.LEVEL_PRIMARIA, allGrades, allEnglishLevels));
        }

        static public IEnumerable<Section> GetSections()
        {
            return sections;
        }

        public static List<string> UserTypes
        {
            get { return allUserTypes; }
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
            return "6to";
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


}




}
