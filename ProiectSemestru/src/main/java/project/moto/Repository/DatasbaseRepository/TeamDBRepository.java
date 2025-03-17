package project.moto.Repository.DatasbaseRepository;

import org.apache.logging.log4j.LogManager;
import org.apache.logging.log4j.Logger;
import project.moto.Domain.Team;
import project.moto.Repository.TeamRepository;
import project.moto.Utils.JdbcUtils;

import java.sql.Connection;
import java.sql.PreparedStatement;
import java.util.*;

public class TeamDBRepository implements TeamRepository {
    private final JdbcUtils dbUtils;
    private static final Logger logger = LogManager.getLogger();
    public TeamDBRepository(Properties props) {
        this.dbUtils = new JdbcUtils(props);
        logger.info("Initialized TeamDBRepository with properties: {}", props);

    }

    /**
     * Find the Team with the given id
     * @param tID -the id of the team to be returned
     * id must not be null
     * @return Team - null if it doesnt exist
     */
    @Override
    public Optional<Team> findOne(Integer tID) {
        Connection con = dbUtils.getConnection();
        try (var preStmt = con.prepareStatement("select * from Team where id = ?")) {
            preStmt.setInt(1, tID);
            try (var result = preStmt.executeQuery()) {
                if (result.next()) {
                    int id = result.getInt("Id");
                    String name = result.getString("Name");
                    Team team = new Team(name);
                    team.setId(id);
                    logger.traceExit("Team Found");
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
        logger.info("Finding Teams from database");
        Connection con = dbUtils.getConnection();
        Map<Integer,Team> data = new HashMap<>();
        try (var preStmt = con.prepareStatement("select * from Team")) {
            try (var result = preStmt.executeQuery()) {
                while (result.next()) {
                    int id = result.getInt("id");
                    String name = result.getString("Name");
                    Team team = new Team(name);
                    team.setId(id);
                    data.put(id,team);
                }
                logger.traceExit("Found {} Teams",data.size());
                return data;
            }
        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
    }

    /**
     * Save a Team to the database
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
     * Delete a Team from the database
     * @param id
     * id must be not null
     * @return Optional<Team> - null if the Team was not successfully deleted
     */
    @Override
    public Optional<Team> delete(Integer id) {
        logger.traceEntry("deleting team {} ", id);
        Connection con = dbUtils.getConnection();
        Optional<Team> team = findOne(id);
        if(team.isPresent()) {
            try (var preStmt = con.prepareStatement("delete from Team where id = ?")) {
                preStmt.setInt(1, id);
                preStmt.executeUpdate();
                logger.traceExit("Deleted team {}", id);
                return team;
            } catch (Exception e) {
                logger.error(e.getMessage());
                throw new RuntimeException(e);
            }
        }else{
            logger.traceExit("Team not found");
            return Optional.empty();
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
            return Optional.of(entity);

        } catch (Exception e) {
            logger.error(e.getMessage());
            throw new RuntimeException(e);
        }
    }
}
