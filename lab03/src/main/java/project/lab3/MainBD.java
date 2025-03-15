package project.lab3;

import java.io.FileReader;
import java.io.IOException;
import java.util.Properties;

public class MainBD {
    public static void main(String[] args) {

        Properties props=new Properties();
        try {
            props.load(new FileReader("bd.config"));
        } catch (IOException e) {
            System.out.println("Cannot find bd.config "+e);
        }

        CarRepository carRepo=new CarsDBRepository(props);
        carRepo.add(new Car("Audi","A4", 2015));
        System.out.println("Toate masinile din db");
        for(Car car:carRepo.findAll())
            System.out.println(car);
       String manufacturer="Dacia";
        System.out.println("Masinile produse de "+manufacturer);
        for(Car car:carRepo.findByManufacturer(manufacturer))
            System.out.println(car);
        System.out.println("Toate masinile intre anii 2013-2020");
        for(Car car:carRepo.findBetweenYears(2013,2020))
            System.out.println(car);
        System.out.println("Toate masinile din db:");
        for(Car car:carRepo.findAll())
            System.out.println(car);
        carRepo.update(4,new Car("Volskwagen","golf 7", 2021));
        System.out.println("Toate masinile din db dupa update:");
        for(Car car:carRepo.findAll())
            System.out.println(car);
    }
}
