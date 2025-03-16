package project.moto.Repository;

import project.moto.Domain.Team;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.util.*;

public class TeamDBRepository extends AbstractDatabaseRepository<Integer, Team> {

    public TeamDBRepository(Properties props) {
        super(props);
        logger.info("Initialized TeamDBRepository with properties: {}", props);
    }

    /**
     * Find the Team with the given id, if it exists in memory in returns instantly, else queries the DB
     * @param aInt -the id of the team to be returned
     * id must not be null
     * @return Team - null if it doesnt exist
     */
    @Override
    public Optional<Team> findOne(Integer aInt) {
        logger.info("Finding Team by ID: {}", aInt);
        if(data.containsKey(aInt))
        {
            logger.traceExit("Team MEMORY_HIT");
            return Optional.of(data.get(aInt));
        }
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("select * from Team where id = ?")) {
            preStmt.setInt(1, aInt);
            try (var result = preStmt.executeQuery()) {
                if (result.next()) {
                    int id = result.getInt("Id");
                    String name = result.getString("Name");
                    Team team = new Team(name);
                    team.setId(id);
                    logger.traceExit();
                    data.put(id, team);
                    return Optional.of(team);
                }
            }
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
        logger.traceExit("Team not found");
        return Optional.empty();
    }

    /**
     * Find all Teams
     * @return Map<Integer,Team> - all Teams
     */
    @Override
    public Map<Integer,Team> findAll() {
        logger.info("Finding all Teams");
        return super.findAll();
    }

    /**
     * Load all Teams from the database
     */
    @Override
    protected void Load(){
        logger.info("Loading Teams from database");
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("select * from Team")) {
            try (var result = preStmt.executeQuery()) {
                while (result.next()) {
                    int id = result.getInt("id");
                    String name = result.getString("Name");
                    Team team = new Team(name);
                    team.setId(id);
                    data.put(id,team);
                }
            }
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
        logger.traceExit("Found {} Teams",data.size());
    }

    /**
     * Save a Team to the database and locally
     * @param entity
     * entity must be not null
     * @return Optional<Team> - null if the Team was not successfully saved
     */
    @Override
    public Optional<Team> save(Team entity) {
        logger.traceEntry("saving team {} ", entity.getId());
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement(
                "insert into Team (Name) values (?)",PreparedStatement.RETURN_GENERATED_KEYS)) {
            preStmt.setString(1, entity.getName());
            var result = preStmt.executeUpdate();
            if (result > 0) {
                try (var generatedKeys = preStmt.getGeneratedKeys()) {
                    if (generatedKeys.next()) {
                        entity.setId(generatedKeys.getInt(1));
                    }
                }
                data.put(entity.getId(), entity);
                logger.traceExit("Team {} saved successfully", entity.getId());
                return Optional.of(entity);
            }
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
        logger.traceExit("Team {} not saved", entity.getId());
        return Optional.empty();
    }

    /**
     * Delete a Team from the database and locally
     * @param integer
     * id must be not null
     * @return Optional<Team> - null if the Team was not successfully deleted
     */
    @Override
    public Optional<Team> delete(Integer integer) {
        logger.traceEntry("deleting team {} ", integer);
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("delete from Team where id = ?")) {
            preStmt.setInt(1, integer);
            preStmt.executeUpdate();
            logger.traceExit("Deleted team {}", integer);
            return Optional.of(data.remove(integer));
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
    }

    /**
     * Update a Team in the database and locally
     * @param entity - entity must not be null
     * @return Optional<Team> - null if the Team was not successfully updated
     */
    @Override
    public Optional<Team> update(Team entity) {
        logger.traceEntry("Updating team {} ", entity.getId());
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("update Team set Name = ? where id = ?")) {
            preStmt.setString(1, entity.getName());
            preStmt.setInt(2, entity.getId());
            preStmt.executeUpdate();
            logger.traceExit("Successfully updated team {} data", entity.getId());
            return Optional.ofNullable(data.put(entity.getId(), entity));

        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
    }
}
