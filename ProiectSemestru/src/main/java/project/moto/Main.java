package project.moto;

import project.moto.Domain.Player;
import project.moto.Domain.Race;
import project.moto.Domain.Team;
import project.moto.Domain.User;
import project.moto.Repository.DatasbaseRepository.PlayerDBRepository;
import project.moto.Repository.DatasbaseRepository.RaceDBRepository;
import project.moto.Repository.DatasbaseRepository.TeamDBRepository;
import project.moto.Repository.DatasbaseRepository.UserDBRepository;

import java.io.FileReader;
import java.io.IOException;
import java.util.List;
import java.util.Optional;
import java.util.Properties;

public class Main {
    public static void main(String[] args) {
        Properties props = new Properties();
        try {
            props.load(new FileReader("bd.config"));
        } catch (IOException e) {
            System.out.println("Cannot find bd.config " + e);
        }

        Team team = new Team("Ferrari");
        Player player = new Player("Jeremy Clarkson", "1110001110001", 3);
        User user = new User("admin", "admin");


        PlayerDBRepository playerDBRepository = new PlayerDBRepository(props);
        System.out.println("Toate playerii din db:");
        playerDBRepository.findAll().forEach((key, value) -> {
            System.out.println(key + " => " + value);
        });

        TeamDBRepository teamDBRepository = new TeamDBRepository(props);
        System.out.println("Toate echipele din db:");
        teamDBRepository.findAll().forEach((key, value) -> {
            System.out.println(key + " => " + value);
        });

        RaceDBRepository raceDBRepository = new RaceDBRepository(props, playerDBRepository);
        System.out.println("Toate cursele din db:");
        raceDBRepository.findAll().forEach((key, value) -> {
            System.out.println(key + " => " + value);
        });

        UserDBRepository userDBRepository = new UserDBRepository(props);
        System.out.println("Toate userii din db:");
        userDBRepository.findAll().forEach((key, value) -> {
            System.out.println(key + " => " + value);
        });

        Optional<User> userOptional = userDBRepository.save(user);
        System.out.println("New User added:"+userDBRepository.findOne(2).get());
        Optional<User> userOptional2 = userDBRepository.findByUsername("horatiu");
        userOptional2.ifPresentOrElse(System.out::println, () -> System.out.println("Userul nu exista in baza de date"));
        Optional<Team> teamOptional = teamDBRepository.save(team);
        teamOptional.ifPresentOrElse(System.out::println, () -> System.out.println("Echipa exista deja in baza de date"));

        Optional<Player> playerOptional = playerDBRepository.save(player);
        playerOptional.ifPresentOrElse(System.out::println, () -> System.out.println("Playerul exista deja in baza de date"));


        Race r = raceDBRepository.findOne(1).get();
        List<Player> players = r.getPlayers();
        players.add(player);
        r.setPlayers(players);
        r.setNoPlayers(players.size());
        raceDBRepository.update(r);
        raceDBRepository.findAll().forEach((key, value) -> {
            System.out.println(key + " => " + value);
        });

    }
}