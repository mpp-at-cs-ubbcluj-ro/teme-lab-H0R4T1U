package project.moto;

import project.moto.Domain.Player;
import project.moto.Repository.PlayerDBRepository;
import project.moto.Repository.RaceDBRepository;
import project.moto.Repository.TeamDBRepository;

import java.io.FileReader;
import java.io.IOException;
import java.util.Optional;
import java.util.Properties;

//TIP To <b>Run</b> code, press <shortcut actionId="Run"/> or
// click the <icon src="AllIcons.Actions.Execute"/> icon in the gutter.
public class Main {
    public static void main(String[] args) {
        Properties props = new Properties();
        try {
            props.load(new FileReader("bd.config"));
        } catch (IOException e) {
            System.out.println("Cannot find bd.config " + e);
        }
        PlayerDBRepository playerDBRepository = new PlayerDBRepository(props);
        System.out.println("Toate playerii din db:");
        playerDBRepository.findAll().forEach(System.out::println);

        TeamDBRepository teamDBRepository = new TeamDBRepository(props);
        System.out.println("Toate echipele din db:");
        teamDBRepository.findAll().forEach(System.out::println);

        RaceDBRepository raceDBRepository = new RaceDBRepository(props);
        System.out.println("Toate cursele din db:");
        raceDBRepository.findAll().forEach(System.out::println);
    }
}