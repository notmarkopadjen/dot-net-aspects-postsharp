namespace Paden.Aspects.DAL.Entities
{
    public class Student
    {
        public const string ReCreateStatement = @"
DROP TABLE IF EXISTS `Students`;
CREATE TABLE `Students`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) CHARACTER SET latin1 COLLATE latin1_swedish_ci NOT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = latin1 COLLATE = latin1_swedish_ci ROW_FORMAT = Dynamic;
";

        public int Id { get; set; }
        public string Name { get; set; }
    }
}
