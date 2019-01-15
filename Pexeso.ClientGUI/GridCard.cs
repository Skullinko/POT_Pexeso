using System.Windows.Controls;

namespace Pexeso.ClientGUI
{
    public class GridCard
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public char Value { get; set; }
        public bool Hidden { get; set; }
        public Label Label { get; set; }

        public GridCard(int row, int column, char value, Label label = null)
        {
            Row = row;
            Column = column;
            Value = value;
            Label = label;
            Hidden = true;
        }

        public override string ToString()
        {
            return Hidden ? "" : Value.ToString();
        }
    }
}
