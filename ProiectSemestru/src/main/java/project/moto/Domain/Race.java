package project.moto.Domain;

import java.util.ArrayList;
import java.util.List;

public class Race extends Entity<Long> {
    private Integer engineType;
    private Integer noPlayers;
    private List<Player> players;

    public Race(Integer engineType) {
        this.engineType = engineType;
        this.noPlayers = 0;
        this.players = new ArrayList<>();

    }

    public Integer getEngineType() {
        return engineType;
    }

    public Integer getNoPlayers() {
        return noPlayers;
    }

    public List<Player> getPlayers() {
        return players;
    }

    public void setPlayers(List<Player> players) {
        this.players = players;
        this.noPlayers = this.players.size();
    }
}
