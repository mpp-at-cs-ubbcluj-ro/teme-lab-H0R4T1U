import ubb.scs.map.model.ComputerRepairRequest;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;


public class ComputerRepairRequestTest {
    
    @Test
    @DisplayName("First Test")
    public void testExample(){
        ComputerRepairRequest crr = new ComputerRepairRequest();
        assertEquals("",crr.getOwnerName());
        assertEquals("",crr.getOwnerAddress());
    }
    @Test
    @DisplayName("second Test")
    public void testExample2(){
        assertEquals(2,2,"numerele ar trebui sa fie egale");
    }

      
}
