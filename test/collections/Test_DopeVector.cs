using System.Runtime.CompilerServices;
using Howl.Collections;

namespace Howl.Test.Collections;

public class Test_DopeVector
{
    [Fact]
    public void Constructor_Test()
    {
        for(int length = 0; length < 8; length++)
        {
            int entryDataLength = length+1;
            DopeVector<float> array = new(length, entryDataLength);
            Assert_DopeVector.LengthEqual(entryDataLength, length, array);
        }
    }

    [Fact]
    public void Append_Test()
    {
        for(int length = 0; length < 8; length++)
        {
            DopeVector<float> array = new(length, length);            
            
            int j = 0;
            for(int dataLength = 0; dataLength < length; dataLength++)
            {                
                for(int entry = 0; entry < length; entry++)
                {
                    float data = j++; 
                    if(DopeVector.Append(array, data, entry))
                    {
                        Assert_DopeVector.EntryEqual(data, dataLength+1, length, entry, array);                    
                    }
                }
            }
        }
    }

    [Fact]
    public void GetAppendedData_Test()
    {   
        for(int length = 0; length < 8; length++)
        {
            DopeVector<float> array = new(length, length);            
            
            int j = 0;
            for(int entry = 0; entry < length; entry++)
            {
                List<float> expected = new();

                for(int dataLength = entry; dataLength < length; dataLength++)
                {                    
                    float data = j++; 
                    if(DopeVector.Append(array, data, entry))
                    {
                        expected.Add(data);
                    }
                }

                Span<float> appended = DopeVector.GetAppendedData(array, entry);

                Assert.Equal(expected.Count, appended.Length);

                for(int i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i], appended[i]);
                }
            }
        }        
    }
}