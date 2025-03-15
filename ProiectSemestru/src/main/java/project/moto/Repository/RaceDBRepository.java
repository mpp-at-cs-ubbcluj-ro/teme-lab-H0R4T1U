package project.moto.Repository;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import project.moto.Utils.JdbcUtils;
import project.moto.Domain.Race;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;
import java.util.Optional;
import java.util.Properties;

public class RaceDBRepository implements Repository<Integer, Race> {
    private final JdbcUtils dbUtils;
    private static final Logger logger = LogManager.getLogger();

    public RaceDBRepository(Properties props) {
        this.dbUtils = new JdbcUtils(props);
        logger.info("Initialized RaceDBRepository with properties: {}", props);
    }

    @Override
    public Optional<Race> findOne(Integer raceId) {
        logger.traceEntry("findOne race with id {}", raceId);
        Connection con = dbUtils.getConnection();
        Race race = null;
        try (PreparedStatement preStmt = con.prepareStatement("SELECT * FROM Race WHERE Id = ?")) {
            preStmt.setInt(1, raceId);
            try (ResultSet result = preStmt.executeQuery()) {
                if (result.next()) {
                    Integer engineType = result.getInt("EngineType");
                    Integer noPlayers = result.getInt("NoPlayers");
                    race = new Race(engineType);
                    race.setId(raceId);
                    race.setNoPlayers(noPlayers);
                }
            }
        } catch (SQLException e) {
            logger.error(e);
            System.err.println("Error DB " + e);
        }

        if (race != null) {
            try (PreparedStatement preStmt = con.prepareStatement("SELECT PlayerId FROM PlayerRaces WHERE RaceId = ?")) {
                preStmt.setInt(1, raceId);
                try (ResultSet result = preStmt.executeQuery()) {
                    List<Long> playerIds = new ArrayList<>();
                    while (result.next()) {
                        playerIds.add(result.getLong("PlayerId"));
                    }
                    race.setPlayerIds(playerIds);
                }
            } catch (SQLException e) {
                logger.error(e);
                System.err.println("Error DB " + e);
            }
        }
        logger.traceExit();
        return Optional.ofNullable(race);
    }

    @Override
    public Iterable<Race> findAll() {
        logger.traceEntry("findAll races");
        Connection con = dbUtils.getConnection();
        List<Race> races = new ArrayList<>();
        try (PreparedStatement preStmt = con.prepareStatement("SELECT * FROM Race")) {
            try (ResultSet result = preStmt.executeQuery()) {
                while (result.next()) {
                    Integer raceId = result.getInt("Id");
                    Integer engineType = result.getInt("EngineType");
                    Integer noPlayers = result.getInt("NoPlayers");
                    Race race = new Race(engineType);
                    race.setId(raceId);
                    race.setNoPlayers(noPlayers);

                    try (PreparedStatement preStmtPlayers = con.prepareStatement("SELECT PlayerId FROM PlayerRaces WHERE RaceId = ?")) {
                        preStmtPlayers.setInt(1, raceId);
                        try (ResultSet resultPlayers = preStmtPlayers.executeQuery()) {
                            List<Long> playerIds = new ArrayList<>();
                            while (resultPlayers.next()) {
                                playerIds.add(resultPlayers.getLong("PlayerId"));
                            }
                            race.setPlayerIds(playerIds);
                        }
                    }
                    races.add(race);
                }
            }
        } catch (SQLException e) {
            logger.error(e);
            System.err.println("Error DB " + e);
        }
        return races;
    }

    @Override
    public Optional<Race> save(Race race) {
        logger.traceEntry("saving race {}", race);
        Connection con = dbUtils.getConnection();
        try (PreparedStatement preStmt = con.prepareStatement("INSERT INTO Race (EngineType, NoPlayers) VALUES (?, ?)", PreparedStatement.RETURN_GENERATED_KEYS)) {
            preStmt.setInt(1, race.getEngineType());
            preStmt.setInt(2, race.getNoPlayers());
            int result = preStmt.executeUpdate();
            if (result > 0) {
                try (ResultSet generatedKeys = preStmt.getGeneratedKeys()) {
                    if (generatedKeys.next()) {
                        race.setId(generatedKeys.getInt(1));
                    }
                }
                for (Long playerId : race.getPlayerIds()) {
                    try (PreparedStatement preStmtPlayers = con.prepareStatement("INSERT INTO PlayerRaces (PlayerId, RaceId) VALUES (?, ?)")) {
                        preStmtPlayers.setLong(1, playerId);
                        preStmtPlayers.setInt(2, race.getId());
                        preStmtPlayers.executeUpdate();
                    }
                }
                return Optional.of(race);
            }
        } catch (SQLException e) {
            logger.error(e);
            System.err.println("Error DB " + e);
        }
        return Optional.empty();
    }

    @Override
    public Optional<Race> delete(Integer raceId) {
        logger.traceEntry("deleting race with id {}", raceId);
        Connection con = dbUtils.getConnection();
        Optional<Race> race = findOne(raceId);
        if (race.isPresent()) {
            try (PreparedStatement preStmtPlayers = con.prepareStatement("DELETE FROM PlayerRaces WHERE RaceId = ?")) {
                preStmtPlayers.setInt(1, raceId);
                preStmtPlayers.executeUpdate();
            } catch (SQLException e) {
                logger.error(e);
                System.err.println("Error DB " + e);
            }

            try (PreparedStatement preStmt = con.prepareStatement("DELETE FROM Race WHERE Id = ?")) {
                preStmt.setInt(1, raceId);
                preStmt.executeUpdate();
            } catch (SQLException e) {
                logger.error(e);
                System.err.println("Error DB " + e);
            }
        }
        return race;
    }

    @Override
    public Optional<Race> update(Race race) {
        logger.traceEntry("updating race {}", race);
        Connection con = dbUtils.getConnection();
        try (PreparedStatement preStmt = con.prepareStatement("UPDATE Race SET engineType = ?, NoPlayers = ? WHERE Id = ?")) {
            preStmt.setInt(1, race.getEngineType());
            preStmt.setInt(2, race.getNoPlayers());
            preStmt.setInt(3, race.getId());
            int result = preStmt.executeUpdate();
            if (result > 0) {
                try (PreparedStatement preStmtDeletePlayers = con.prepareStatement("DELETE FROM PlayerRaces WHERE RaceId = ?")) {
                    preStmtDeletePlayers.setInt(1, race.getId());
                    preStmtDeletePlayers.executeUpdate();
                }
                for (Long playerId : race.getPlayerIds()) {
                    try (PreparedStatement preStmtPlayers = con.prepareStatement("INSERT INTO PlayerRaces (PlayerId, RaceId) VALUES (?, ?)")) {
                        preStmtPlayers.setLong(1, playerId);
                        preStmtPlayers.setInt(2, race.getId());
                        preStmtPlayers.executeUpdate();
                    }
                }
                return Optional.of(race);
            }
        } catch (SQLException e) {
            logger.error(e);
            System.err.println("Error DB " + e);
        }
        return Optional.empty();
    }

}
