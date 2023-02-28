using System.Collections;

namespace FifaLibrary
{
    public class RecordComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            Record record = (Record)x;
            Record record2 = (Record)y;
            TableDescriptor tableDescriptor = record.TableDescriptor;
            int num = 0;
            for (int i = 0; i < tableDescriptor.NKeyFields; i++)
            {
                num = tableDescriptor.KeyIndex[i];
                if (record.IntField[num] != record2.IntField[num])
                {
                    if (record.IntField[num] <= record2.IntField[num])
                    {
                        return -1;
                    }
                    return 1;
                }
            }
            return 0;
        }
    }
}
