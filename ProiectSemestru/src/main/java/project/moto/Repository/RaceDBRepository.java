package project.moto.Repository;


import project.moto.Domain.Race;
import project.moto.Domain.Player;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.*;

public class RaceDBRepository extends AbstractDatabaseRepository<Integer,Race> {
    private final PlayerDBRepository playerRepo;

    public RaceDBRepository(Properties props, PlayerDBRepository playerRepo) {
        super(props);
        this.playerRepo = playerRepo;
        logger.info("Initialized RaceDBRepository with properties: {}", props);
    }

    /**
     * Find the race with the given id, if it exists in memory in returns instantly, else queries the database
     * @param raceId -the id of the entity to be returned
     * id must not be null
     * @return Race - null if it doesnt exist
     */
    @Override
    public Optional<Race> findOne(Integer raceId) {
        logger.traceEntry("findOne race with id {}", raceId);
        if(data.containsKey(raceId))
        {
            logger.traceExit("Race MEMORY_HIT");
            return Optional.of(data.get(raceId));
        }
        Connection con = dbUtils.getConnection();
        Race race =  new Race(0);
        try (PreparedStatement preStmt = con.prepareStatement("SELECT * FROM Race WHERE Id = ?")) {
            preStmt.setInt(1, raceId);
            try (ResultSet result = preStmt.executeQuery()) {
                if (result.next()) {
                    Integer engineType = result.getInt("EngineType");
                    Integer noPlayers = result.getInt("NoPlayers");
                    race.setEngineType(engineType);
                    race.setId(raceId);
                    race.setNoPlayers(noPlayers);
                }
            }
        } catch (SQLException e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }

        if (race.getId() != null) {
            try (PreparedStatement preStmt = con.prepareStatement("SELECT PlayerId FROM PlayerRaces WHERE RaceId = ?")) {
                preStmt.setInt(1, raceId);
                try (ResultSet result = preStmt.executeQuery()) {
                    List<Player> players = new ArrayList<>();
                    while (result.next()) {
                        Integer playerId = result.getInt("PlayerId");
                        playerRepo.findOne(playerId).ifPresent(players::add);
                    }
                    race.setPlayers(players);
                    data.put(raceId, race);
                    logger.traceExit("Race found in Database!");
                    return Optional.of(race);
                }
            } catch (SQLException e) {
                logger.error(e.getMessage());
                throw new RuntimeException(e);
            }
        }
        logger.traceExit("Race not found");
        return Optional.empty();
    }

    /**
     *  Returns all teams
     * @return Map<Integer,Team> - all teams
     */
    @Override
    public Map<Integer,Race> findAll() {
        logger.traceEntry("Finding all races");
        return super.findAll();
    }

    /**
     * Returns all data from Database
     */
    @Override
    protected void Load(){
        logger.traceEntry("findAll races");
        Connection con = dbUtils.getConnection();

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
                            List<Player> players = new ArrayList<>();
                            while (resultPlayers.next()) {
                                Integer playerId = resultPlayers.getInt("PlayerId");
                                playerRepo.findOne(playerId).ifPresent(players::add);
                            }
                            race.setPlayers(players);
                        }
                    }
                    data.put(raceId,race);
                }
            }
        } catch (SQLException e) {
            logger.error(e.getMessage());
            System.err.println("Error DB " + e);
        }
    }

    /**
     * Saves race to the database and locally, stops commits until all changes are applied
     * to ensure data consistency
     * @param race
     * entity must be not null
     * @return Optional<Race> - null if the entity was not saved
     */
    public Optional<Race> save(Race race) {
        logger.traceEntry("saving race {}", race);
        Connection con = dbUtils.getConnection();
        boolean oldAutoCommit = true;
        try {
            oldAutoCommit = con.getAutoCommit();
            con.setAutoCommit(false);

            // Insert into Race table
            try (PreparedStatement preStmt = con.prepareStatement(
                    "INSERT INTO Race (EngineType, NoPlayers) VALUES (?, ?)",
                    PreparedStatement.RETURN_GENERATED_KEYS)) {
                preStmt.setInt(1, race.getEngineType());
                preStmt.setInt(2, race.getNoPlayers());
                int result = preStmt.executeUpdate();
                if (result > 0) {
                    try (ResultSet generatedKeys = preStmt.getGeneratedKeys()) {
                        if (generatedKeys.next()) {
                            race.setId(generatedKeys.getInt(1));
                        }
                    }
                    // Insert into PlayerRaces
                    for (Player player : race.getPlayers()) {
                        try (PreparedStatement preStmtPlayers = con.prepareStatement(
                                "INSERT INTO PlayerRaces (PlayerId, RaceId) VALUES (?, ?)")) {
                            preStmtPlayers.setInt(1, player.getId());
                            preStmtPlayers.setInt(2, race.getId());
                            preStmtPlayers.executeUpdate();
                        }
                    }
                    data.put(race.getId(), race);
                    con.commit();
                    logger.traceExit("Race saved with id {}", race.getId());
                    return Optional.of(race);
                }
            }
            logger.traceExit("Race not Added");
            return Optional.empty();
        } catch (SQLException e) {
            try {
                con.rollback();
            } catch (SQLException ex) {
                logger.error(ex.getMessage());
            }
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        } finally {
            try {
                con.setAutoCommit(oldAutoCommit);
            } catch (SQLException e) {
                logger.error(e.getMessage());
            }
        }
    }

    /**
     * Deletes a race locally and from the database if it exists, stops commits until all changes are applied
     * to ensure data consistency
     * @param raceId
     * id must be not null
     * @return Optional<Race> Race - null if it doesn't exist
     */
    public Optional<Race> delete(Integer raceId) {
        logger.traceEntry("deleting race with id {}", raceId);
        Connection con = dbUtils.getConnection();
        Optional<Race> race = findOne(raceId);
        if (race.isPresent()) {
            boolean oldAutoCommit = true;
            try {
                oldAutoCommit = con.getAutoCommit();
                con.setAutoCommit(false);

                // Deletes from PlayerRaces
                try (PreparedStatement preStmt = con.prepareStatement("DELETE FROM PlayerRaces WHERE RaceId = ?")) {
                    preStmt.setInt(1, raceId);
                    preStmt.executeUpdate();
                }

                // Deletes from Race
                try (PreparedStatement preStmt = con.prepareStatement("DELETE FROM Race WHERE Id = ?")) {
                    preStmt.setInt(1, raceId);
                    preStmt.executeUpdate();
                }

                Optional<Race> removed = Optional.of(data.remove(raceId));
                con.commit();
                logger.traceExit("Race deleted with id {}", raceId);
                return removed;
            } catch (SQLException e) {
                try {
                    con.rollback();
                } catch (SQLException ex) {
                    logger.error(ex.getMessage());
                }
                logger.error(e.getMessage());
                throw new RuntimeException(e);
            } finally {
                try {
                    con.setAutoCommit(oldAutoCommit);
                } catch (SQLException e) {
                    logger.error(e.getMessage());
                }
            }
        }
        logger.traceExit("Race not Deleted");
        return Optional.empty();
    }

    /**
     * Updates Race in the database and locally, stops commits until all changes are applied
     * to ensure data consistency
     * @param race - entity must be not null
     * @return Optional<Race> - null if the entity was not updated
     */
    public Optional<Race> update(Race race) {
        logger.traceEntry("updating race {}", race);
        Connection con = dbUtils.getConnection();
        boolean oldAutoCommit = true;
        try {
            oldAutoCommit = con.getAutoCommit();
            con.setAutoCommit(false);

            try (PreparedStatement preStmt = con.prepareStatement(
                    "UPDATE Race SET EngineType = ?, NoPlayers = ? WHERE Id = ?")) {
                preStmt.setInt(1, race.getEngineType());
                preStmt.setInt(2, race.getNoPlayers());
                preStmt.setInt(3, race.getId());
                int result = preStmt.executeUpdate();
                if (result > 0) {
                    try (PreparedStatement preStmtDel = con.prepareStatement(
                            "DELETE FROM PlayerRaces WHERE RaceId = ?")) {
                        preStmtDel.setInt(1, race.getId());
                        preStmtDel.executeUpdate();
                    }

                    for (Player player : race.getPlayers()) {
                        try (PreparedStatement preStmtPlayers = con.prepareStatement(
                                "INSERT INTO PlayerRaces (PlayerId, RaceId) VALUES (?, ?)")) {
                            preStmtPlayers.setInt(1, player.getId());
                            preStmtPlayers.setInt(2, race.getId());
                            preStmtPlayers.executeUpdate();
                        }
                    }

                    con.commit();
                    logger.traceExit("Race updated with id {}", race.getId());
                    return Optional.ofNullable(data.put(race.getId(), race));
                }
            }
            logger.traceExit("Race not Updated");
            return Optional.empty();
        } catch (SQLException e) {
            try {
                con.rollback();
            } catch (SQLException ex) {
                logger.error(ex.getMessage());
            }
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        } finally {
            try {
                con.setAutoCommit(oldAutoCommit);
            } catch (SQLException e) {
                logger.error(e.getMessage());
            }
        }
    }
}