package project.moto.Domain;

import java.util.ArrayList;
import java.util.List;
import java.util.Objects;

public class Race extends Entity<Integer> {
    private Integer engineType;
    private Integer noPlayers;
    private List<Long> playerIds;

    public Race(Integer engineType) {
        this.engineType = engineType;
        this.noPlayers = 0;
        this.playerIds = new ArrayList<>();
    }

    public Integer getEngineType() {
        return engineType;
    }

    public Integer getNoPlayers() {
        return noPlayers;
    }

    public List<Long> getPlayerIds() {
        return playerIds;
    }

    public void setNoPlayers(Integer noPlayers) {
        this.noPlayers = noPlayers;
    }

    public void setPlayerIds(List<Long> playerIds) {
        this.playerIds = playerIds;
    }

    @Override
    public String toString() {
        return "Race{" +
                "engineType=" + engineType +
                ", noPlayers=" + noPlayers +
                ", playerIds=" + playerIds +
                ", id=" + id +
                '}';
    }

    @Override
    public boolean equals(Object o) {
        if (o == null || getClass() != o.getClass()) return false;
        if (!super.equals(o)) return false;
        Race race = (Race) o;
        return Objects.equals(engineType, race.engineType) && Objects.equals(noPlayers, race.noPlayers) && Objects.equals(playerIds, race.playerIds);
    }

    @Override
    public int hashCode() {
        return Objects.hash(super.hashCode(), engineType, noPlayers, playerIds);
    }
}