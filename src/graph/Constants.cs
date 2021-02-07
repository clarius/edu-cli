using System;
using System.Collections.Generic;
using System.Text;

namespace Clarius.Edu.Graph
{
    public class Constants
    {
        public static readonly Guid STUDENT_LICENSE = new Guid("314c4481-f395-4525-be8b-2ec4bb1e9d91");
        public static readonly Guid TEACHER_LICENSE = new Guid("94763226-9b3c-4e75-a931-5c89701abe66");
        public static readonly string STUDENT_LICENSE_NAME = "STUDENT LICENSE";
        public static readonly string TEACHER_LICENSE_NAME = "TEACHER LICENSE";
        public static readonly string UNKNOWN_LICENSE_NAME = "UNKNOWN LICENSE";

        public const string USER_TYPE_STUDENT = "Estudiante";
        public const string USER_TYPE_TEACHER = "Docente";
        public const string USER_TYPE_PRECEPTOR = "Preceptor";
        public const string USER_TYPE_FUNDACION = "Fundacion";
        public const string USER_TYPE_IT = "IT";
        public const string USER_TYPE_TEST = "Test";
        public const string USER_TYPE_EGRESADO = "Egresado";

        public const string DIVISION_A = "A";
        public const string DIVISION_B = "B";
        public const string DIVISION_C = "C";

        public const string ENGLISH_LEVEL_1 = "Nivel 1";
        public const string ENGLISH_LEVEL_2 = "Nivel 2";
        public const string ENGLISH_LEVEL_3 = "Nivel 3";

        public const string GRADE_FIRST = "1ero";
        public const string GRADE_SECOND = "2do";
        public const string GRADE_THIRD = "3ero";
        public const string GRADE_FOURTH = "4to";
        public const string GRADE_FIFTH = "5to";
        public const string GRADE_SIXTH = "6to";

        public const string GROUP_TYPE_CLASS = "Clase";

        public const string LEVEL_PRIMARIA = "Primaria";
        public const string LEVEL_SECUNDARIA = "Secundaria";
        public const string LEVEL_INICIAL = "Inicial";

        public const string PROFILE_GROUPEXTENSION_ID = "edugraph.group.profile";
        public const string PROFILE_GROUPTYPE = "GroupType"; // class? staff? security?
        public const string PROFILE_GROUPGRADE = "GroupGrade"; // 3ero, 5to, etc
        public const string PROFILE_GROUPDIVISION = "GroupDivision"; // A, B, C, etc
        public const string PROFILE_GROUPLEVEL = "GroupLevel"; // Primaria, Secundaria, Inicial
        public const string PROFILE_GROUPYEAR = "GroupYear"; // 2020, 2021, etc (Promocion)
        public const string PROFILE_GROUPID = "GroupId"; // this should allow us to not depend on displayname which a teacher may tweak...

        public const string PROFILE_USEREXTENSION_ID = "edugraph.internal.profile";
        public const string PROFILE_USERTYPE = "UserType"; // student or teacher?
        public const string PROFILE_USERGRADE = "UserGrade"; // 3ero, 5to, etc
        public const string PROFILE_USERDIVISION = "UserDivision"; // A, B, C, etc
        public const string PROFILE_USERLEVEL = "UserLevel"; // Primaria, Secundaria, Inicial
        public const string PROFILE_USERENGLISHLEVEL = "UserLanguageLevel"; // 1, 2, 3, etc
        public const string PROFILE_USERYEAR = "UserYear"; // Mostly used as a flag for student users when promoting them, i.e. take all users with Year 2020 and promote them (by moving them to a higher grade/same division) and then update year to 2021
        public const string PROFILE_USERNATIONALID = "UserNationalID";
    }

    public enum CommandTarget { User, Group, Other };
}
